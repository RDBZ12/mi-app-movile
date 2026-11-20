-- =====================================================================
-- MI JARDÍN — Esquema de base de datos para Supabase (PostgreSQL)
-- Cada planta tiene una historia. Cuida la tuya.
--
-- Cómo ejecutarlo: Supabase → SQL Editor → New query → pegar TODO este
-- archivo → Run. Es idempotente: se puede volver a ejecutar sin romper
-- los datos existentes.
--
-- Tablas: perfiles, plantas, registros_cuidado, recordatorios
-- Almacenamiento: bucket privado "fotos-plantas" (una carpeta por usuario)
-- Seguridad: RLS activo en todas las tablas y en el bucket.
-- =====================================================================

begin;

-- ---------------------------------------------------------------------
-- 1. Función común: mantener "actualizado_en"
-- ---------------------------------------------------------------------
create or replace function public.establecer_actualizado_en()
returns trigger
language plpgsql
set search_path = ''
as $$
begin
  new.actualizado_en := now();
  return new;
end;
$$;

-- ---------------------------------------------------------------------
-- 2. perfiles  (1 a 1 con auth.users)
-- ---------------------------------------------------------------------
create table if not exists public.perfiles (
  id              uuid primary key references auth.users (id) on delete cascade,
  nombre_completo text not null default ''
                  check (char_length(nombre_completo) <= 120),
  correo          text check (char_length(correo) <= 320),
  idioma          text not null default 'es' check (idioma in ('es', 'en')),
  tema            text not null default 'sistema'
                  check (tema in ('claro', 'oscuro', 'sistema')),
  creado_en       timestamptz not null default now(),
  actualizado_en  timestamptz not null default now()
);

comment on table public.perfiles is
  'Perfil de cada usuario (1 a 1 con auth.users). Guarda idioma y tema.';

-- ---------------------------------------------------------------------
-- 3. plantas
--    categoria: texto libre (claves: interior, exterior, suculentas,
--    cactus, medicinales, ornamentales, aromaticas, arboles, otras).
--    No lleva CHECK a propósito: así se pueden añadir categorías nuevas
--    sin cambiar la base de datos.
--    foto_ruta: ruta dentro del bucket (nunca Base64 ni URL).
-- ---------------------------------------------------------------------
create table if not exists public.plantas (
  id                    uuid primary key default gen_random_uuid(),
  usuario_id            uuid not null references public.perfiles (id) on delete cascade,
  nombre                text not null
                        check (char_length(btrim(nombre)) between 1 and 100),
  nombre_cientifico     text check (char_length(nombre_cientifico) <= 150),
  categoria             text not null default 'otras'
                        check (char_length(btrim(categoria)) between 1 and 60),
  foto_ruta             text check (char_length(foto_ruta) <= 300),
  descripcion           text check (char_length(descripcion) <= 2000),
  fecha_adquisicion     date,
  frecuencia_riego_dias smallint check (frecuencia_riego_dias between 1 and 365),
  necesidad_luz         text
                        check (necesidad_luz in ('baja', 'media', 'alta', 'luz_directa')),
  tipo_suelo            text check (char_length(tipo_suelo) <= 100),
  temperatura_min_c     numeric(4, 1),
  temperatura_max_c     numeric(4, 1),
  notas                 text check (char_length(notas) <= 4000),
  es_favorita           boolean not null default false,
  ultimo_riego          timestamptz,
  creado_en             timestamptz not null default now(),
  actualizado_en        timestamptz not null default now(),

  -- Permite que cuidados y recordatorios referencien (planta, usuario)
  -- y así sea imposible mezclar datos de dos usuarios.
  constraint plantas_id_usuario_unico unique (id, usuario_id),
  constraint plantas_temperatura_valida check (
    (temperatura_min_c is null or temperatura_min_c between -30 and 60)
    and (temperatura_max_c is null or temperatura_max_c between -30 and 60)
    and (temperatura_min_c is null or temperatura_max_c is null
         or temperatura_min_c <= temperatura_max_c)
  ),
  -- La foto debe estar dentro de la carpeta del propio usuario.
  constraint plantas_foto_en_carpeta_propia check (
    foto_ruta is null or foto_ruta like usuario_id::text || '/%'
  )
);

comment on table public.plantas is
  'Plantas del usuario. ultimo_riego lo mantiene un trigger; el próximo riego se calcula como ultimo_riego (o fecha_adquisicion) + frecuencia_riego_dias.';

create index if not exists plantas_usuario_idx
  on public.plantas (usuario_id, nombre);
create index if not exists plantas_usuario_categoria_idx
  on public.plantas (usuario_id, categoria);
create index if not exists plantas_favoritas_idx
  on public.plantas (usuario_id) where es_favorita;

-- ---------------------------------------------------------------------
-- 4. registros_cuidado  (historial)
-- ---------------------------------------------------------------------
create table if not exists public.registros_cuidado (
  id             uuid primary key default gen_random_uuid(),
  usuario_id     uuid not null,
  planta_id      uuid not null,
  tipo           text not null check (tipo in (
                   'riego', 'fertilizacion', 'poda', 'trasplante', 'limpieza',
                   'control_plagas', 'cambio_ubicacion', 'otro')),
  fecha_hora     timestamptz not null default now(),
  descripcion    text check (char_length(descripcion) <= 1000),
  observaciones  text check (char_length(observaciones) <= 2000),
  foto_ruta      text check (char_length(foto_ruta) <= 300),
  creado_en      timestamptz not null default now(),
  actualizado_en timestamptz not null default now(),

  constraint registros_cuidado_planta_fk
    foreign key (planta_id, usuario_id)
    references public.plantas (id, usuario_id) on delete cascade,
  constraint registros_cuidado_foto_en_carpeta_propia check (
    foto_ruta is null or foto_ruta like usuario_id::text || '/%'
  )
);

comment on table public.registros_cuidado is
  'Historial de cuidados. Solo se crean cuando el usuario los registra.';

create index if not exists registros_planta_fecha_idx
  on public.registros_cuidado (planta_id, fecha_hora desc);
create index if not exists registros_usuario_fecha_idx
  on public.registros_cuidado (usuario_id, fecha_hora desc);
create index if not exists registros_riegos_idx
  on public.registros_cuidado (planta_id, fecha_hora desc) where tipo = 'riego';

-- ---------------------------------------------------------------------
-- 5. recordatorios
--    fecha_hora es el instante de la próxima ejecución (con zona horaria).
--    Esta tabla solo guarda la intención: la notificación real la
--    programa el dispositivo (Android) o el navegador.
-- ---------------------------------------------------------------------
create table if not exists public.recordatorios (
  id             uuid primary key default gen_random_uuid(),
  usuario_id     uuid not null,
  planta_id      uuid not null,
  tipo_cuidado   text not null default 'riego' check (tipo_cuidado in (
                   'riego', 'fertilizacion', 'poda', 'trasplante', 'otro')),
  titulo         text not null check (char_length(btrim(titulo)) between 1 and 120),
  descripcion    text check (char_length(descripcion) <= 1000),
  fecha_hora     timestamptz not null,
  repeticion     text not null default 'ninguna'
                 check (repeticion in ('ninguna', 'diaria', 'semanal', 'quincenal', 'mensual')),
  estado         text not null default 'pendiente'
                 check (estado in ('pendiente', 'completado', 'desactivado')),
  completado_en  timestamptz,
  creado_en      timestamptz not null default now(),
  actualizado_en timestamptz not null default now(),

  constraint recordatorios_planta_fk
    foreign key (planta_id, usuario_id)
    references public.plantas (id, usuario_id) on delete cascade
);

comment on table public.recordatorios is
  'Recordatorios de cuidados. Para completarlos usa la función completar_recordatorio().';

create index if not exists recordatorios_usuario_estado_idx
  on public.recordatorios (usuario_id, estado, fecha_hora);
create index if not exists recordatorios_planta_idx
  on public.recordatorios (planta_id);

-- ---------------------------------------------------------------------
-- 6. Triggers de "actualizado_en"
-- ---------------------------------------------------------------------
drop trigger if exists perfiles_actualizado_en on public.perfiles;
create trigger perfiles_actualizado_en before update on public.perfiles
  for each row execute function public.establecer_actualizado_en();

drop trigger if exists plantas_actualizado_en on public.plantas;
create trigger plantas_actualizado_en before update on public.plantas
  for each row execute function public.establecer_actualizado_en();

drop trigger if exists registros_cuidado_actualizado_en on public.registros_cuidado;
create trigger registros_cuidado_actualizado_en before update on public.registros_cuidado
  for each row execute function public.establecer_actualizado_en();

drop trigger if exists recordatorios_actualizado_en on public.recordatorios;
create trigger recordatorios_actualizado_en before update on public.recordatorios
  for each row execute function public.establecer_actualizado_en();

-- ---------------------------------------------------------------------
-- 7. Último riego: se recalcula a partir del historial (correcto aunque
--    el usuario edite o elimine registros).
-- ---------------------------------------------------------------------
create or replace function public.sincronizar_ultimo_riego()
returns trigger
language plpgsql
set search_path = ''
as $$
begin
  if tg_op in ('UPDATE', 'DELETE') then
    update public.plantas p
       set ultimo_riego = (
         select max(r.fecha_hora)
           from public.registros_cuidado r
          where r.planta_id = old.planta_id and r.tipo = 'riego')
     where p.id = old.planta_id;
  end if;

  if tg_op in ('INSERT', 'UPDATE') then
    update public.plantas p
       set ultimo_riego = (
         select max(r.fecha_hora)
           from public.registros_cuidado r
          where r.planta_id = new.planta_id and r.tipo = 'riego')
     where p.id = new.planta_id;
  end if;

  return null;
end;
$$;

drop trigger if exists registros_cuidado_ultimo_riego on public.registros_cuidado;
create trigger registros_cuidado_ultimo_riego
  after insert or delete or update of tipo, fecha_hora, planta_id
  on public.registros_cuidado
  for each row execute function public.sincronizar_ultimo_riego();

-- ---------------------------------------------------------------------
-- 8. Completar un recordatorio (calcula la siguiente fecha si se repite)
--    - Sin repetición: pasa a "completado".
--    - Con repetición: sigue "pendiente" y avanza a la próxima fecha
--      futura (salta las que ya pasaron). Usa los permisos y el RLS de
--      quien la llama.
--    Uso desde el cliente: supabase.rpc('completar_recordatorio',
--                                       { p_recordatorio_id: '<uuid>' })
-- ---------------------------------------------------------------------
create or replace function public.completar_recordatorio(p_recordatorio_id uuid)
returns public.recordatorios
language plpgsql
set search_path = ''
as $$
declare
  r         public.recordatorios;
  paso      interval;
  siguiente timestamptz;
begin
  select * into r
    from public.recordatorios
   where id = p_recordatorio_id
     for update;

  if not found then
    raise exception 'Recordatorio no encontrado' using errcode = 'P0002';
  end if;

  if r.estado <> 'pendiente' then
    raise exception 'Solo se puede completar un recordatorio pendiente'
      using errcode = '22023';
  end if;

  if r.repeticion = 'ninguna' then
    update public.recordatorios
       set estado = 'completado', completado_en = now()
     where id = r.id
    returning * into r;
  else
    paso := case r.repeticion
              when 'diaria'    then interval '1 day'
              when 'semanal'   then interval '7 days'
              when 'quincenal' then interval '14 days'
              when 'mensual'   then interval '1 month'
            end;
    siguiente := r.fecha_hora + paso;
    while siguiente <= now() loop
      siguiente := siguiente + paso;
    end loop;

    update public.recordatorios
       set fecha_hora = siguiente, completado_en = now()
     where id = r.id
    returning * into r;
  end if;

  return r;
end;
$$;

-- ---------------------------------------------------------------------
-- 9. Perfil automático al registrarse (correo o Google)
-- ---------------------------------------------------------------------
create or replace function public.crear_perfil_nuevo_usuario()
returns trigger
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_idioma text := new.raw_user_meta_data ->> 'idioma';
begin
  insert into public.perfiles (id, nombre_completo, correo, idioma)
  values (
    new.id,
    left(coalesce(
      nullif(btrim(new.raw_user_meta_data ->> 'nombre_completo'), ''),
      nullif(btrim(new.raw_user_meta_data ->> 'full_name'), ''),
      nullif(btrim(new.raw_user_meta_data ->> 'name'), ''),
      ''), 120),
    new.email,
    case when v_idioma in ('es', 'en') then v_idioma else 'es' end
  )
  on conflict (id) do nothing;
  return new;
end;
$$;

revoke execute on function public.crear_perfil_nuevo_usuario() from public, anon, authenticated;

drop trigger if exists al_crear_usuario_auth on auth.users;
create trigger al_crear_usuario_auth
  after insert on auth.users
  for each row execute function public.crear_perfil_nuevo_usuario();

-- Perfiles para usuarios que ya existieran antes de ejecutar este script.
insert into public.perfiles (id, nombre_completo, correo)
select u.id,
       left(coalesce(
         nullif(btrim(u.raw_user_meta_data ->> 'nombre_completo'), ''),
         nullif(btrim(u.raw_user_meta_data ->> 'full_name'), ''),
         nullif(btrim(u.raw_user_meta_data ->> 'name'), ''),
         ''), 120),
       u.email
  from auth.users u
on conflict (id) do nothing;

-- ---------------------------------------------------------------------
-- 10. Permisos de tabla y Row Level Security
-- ---------------------------------------------------------------------
alter table public.perfiles          enable row level security;
alter table public.plantas           enable row level security;
alter table public.registros_cuidado enable row level security;
alter table public.recordatorios     enable row level security;

revoke all on public.perfiles, public.plantas,
              public.registros_cuidado, public.recordatorios
  from anon, authenticated;

grant select on public.perfiles to authenticated;
grant update (nombre_completo, idioma, tema) on public.perfiles to authenticated;
grant select, insert, update, delete
  on public.plantas, public.registros_cuidado, public.recordatorios
  to authenticated;

revoke execute on function public.completar_recordatorio(uuid) from public, anon;
grant  execute on function public.completar_recordatorio(uuid) to authenticated;

-- perfiles: solo el propio perfil
drop policy if exists perfiles_select on public.perfiles;
create policy perfiles_select on public.perfiles
  for select to authenticated
  using (id = (select auth.uid()));

drop policy if exists perfiles_update on public.perfiles;
create policy perfiles_update on public.perfiles
  for update to authenticated
  using (id = (select auth.uid()))
  with check (id = (select auth.uid()));

-- plantas
drop policy if exists plantas_select on public.plantas;
create policy plantas_select on public.plantas
  for select to authenticated using (usuario_id = (select auth.uid()));

drop policy if exists plantas_insert on public.plantas;
create policy plantas_insert on public.plantas
  for insert to authenticated with check (usuario_id = (select auth.uid()));

drop policy if exists plantas_update on public.plantas;
create policy plantas_update on public.plantas
  for update to authenticated
  using (usuario_id = (select auth.uid()))
  with check (usuario_id = (select auth.uid()));

drop policy if exists plantas_delete on public.plantas;
create policy plantas_delete on public.plantas
  for delete to authenticated using (usuario_id = (select auth.uid()));

-- registros_cuidado (la FK compuesta garantiza que la planta es del usuario)
drop policy if exists registros_select on public.registros_cuidado;
create policy registros_select on public.registros_cuidado
  for select to authenticated using (usuario_id = (select auth.uid()));

drop policy if exists registros_insert on public.registros_cuidado;
create policy registros_insert on public.registros_cuidado
  for insert to authenticated with check (usuario_id = (select auth.uid()));

drop policy if exists registros_update on public.registros_cuidado;
create policy registros_update on public.registros_cuidado
  for update to authenticated
  using (usuario_id = (select auth.uid()))
  with check (usuario_id = (select auth.uid()));

drop policy if exists registros_delete on public.registros_cuidado;
create policy registros_delete on public.registros_cuidado
  for delete to authenticated using (usuario_id = (select auth.uid()));

-- recordatorios
drop policy if exists recordatorios_select on public.recordatorios;
create policy recordatorios_select on public.recordatorios
  for select to authenticated using (usuario_id = (select auth.uid()));

drop policy if exists recordatorios_insert on public.recordatorios;
create policy recordatorios_insert on public.recordatorios
  for insert to authenticated with check (usuario_id = (select auth.uid()));

drop policy if exists recordatorios_update on public.recordatorios;
create policy recordatorios_update on public.recordatorios
  for update to authenticated
  using (usuario_id = (select auth.uid()))
  with check (usuario_id = (select auth.uid()));

drop policy if exists recordatorios_delete on public.recordatorios;
create policy recordatorios_delete on public.recordatorios
  for delete to authenticated using (usuario_id = (select auth.uid()));

-- ---------------------------------------------------------------------
-- 11. Storage: bucket privado "fotos-plantas"
--     Ruta: <usuario_uuid>/plantas/<archivo>.webp
--           <usuario_uuid>/cuidados/<archivo>.webp
--     Límite 5 MB; solo JPEG, PNG y WebP. Se leen con URLs firmadas.
-- ---------------------------------------------------------------------
insert into storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
values ('fotos-plantas', 'fotos-plantas', false, 5242880,
        array['image/jpeg', 'image/png', 'image/webp'])
on conflict (id) do update
  set public             = false,
      file_size_limit    = excluded.file_size_limit,
      allowed_mime_types = excluded.allowed_mime_types;

drop policy if exists fotos_plantas_select on storage.objects;
create policy fotos_plantas_select on storage.objects
  for select to authenticated
  using (bucket_id = 'fotos-plantas'
         and (storage.foldername(name))[1] = (select auth.uid())::text);

drop policy if exists fotos_plantas_insert on storage.objects;
create policy fotos_plantas_insert on storage.objects
  for insert to authenticated
  with check (bucket_id = 'fotos-plantas'
              and (storage.foldername(name))[1] = (select auth.uid())::text);

drop policy if exists fotos_plantas_update on storage.objects;
create policy fotos_plantas_update on storage.objects
  for update to authenticated
  using (bucket_id = 'fotos-plantas'
         and (storage.foldername(name))[1] = (select auth.uid())::text)
  with check (bucket_id = 'fotos-plantas'
              and (storage.foldername(name))[1] = (select auth.uid())::text);

drop policy if exists fotos_plantas_delete on storage.objects;
create policy fotos_plantas_delete on storage.objects
  for delete to authenticated
  using (bucket_id = 'fotos-plantas'
         and (storage.foldername(name))[1] = (select auth.uid())::text);

commit;
