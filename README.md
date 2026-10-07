# Biblioteca — ASP.NET Core MVC + Dapper

Sistema web de gestión de biblioteca construido como laboratorio de la Semana 08 del curso **DAEA (Desarrollo de Aplicaciones Empresariales Avanzadas)** — TECSUP. Permite administrar el catálogo de libros, el registro de socios y el ciclo completo de préstamos (creación, consulta por reporte y devolución), usando **ASP.NET Core MVC** en la capa web y **Dapper** sobre **SQL Server** con **procedimientos almacenados** como único mecanismo de acceso a datos.

## Tabla de contenido

- [Objetivo del laboratorio](#objetivo-del-laboratorio)
- [Stack tecnológico](#stack-tecnológico)
- [Arquitectura](#arquitectura)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Modelo de base de datos](#modelo-de-base-de-datos)
- [Procedimientos almacenados](#procedimientos-almacenados)
- [Funcionalidades](#funcionalidades)
- [Reglas de negocio](#reglas-de-negocio)
- [Puesta en marcha](#puesta-en-marcha)
- [Flujo de una petición](#flujo-de-una-petición)
- [Explicación: recorrido completo de una petición (caso Editar libro)](#explicación-recorrido-completo-de-una-petición-caso-editar-libro)
- [Notas de diseño](#notas-de-diseño)
- [Observaciones y conclusiones](#observaciones-y-conclusiones)

## Objetivo del laboratorio

Reutilizar la base de datos `BibliotecaDB` creada en una semana anterior y exponerla a través de una aplicación **ASP.NET Core Web App (Model-View-Controller)**, aplicando:

- Acceso a datos 100% vía **Dapper + procedimientos almacenados** (sin SQL embebido en controladores, sin Entity Framework).
- Separación estricta de capas: **Controllers → Repositorios → Stored Procedures**.
- Operaciones **asíncronas de punta a punta** (`async`/`await`, sin `.Result` ni `.Wait()`).
- Validación con **DataAnnotations** y manejo de errores de negocio con `ModelState`.
- Patrón **Post/Redirect/Get** con mensajes de confirmación vía `TempData`.
- Eliminación **lógica** (campo `Activo`), nunca `DELETE` físico.

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Framework web | ASP.NET Core MVC (.NET 10, `net10.0`) |
| Acceso a datos | [Dapper](https://github.com/DapperLib/Dapper) 2.1.89 (micro-ORM sobre `IDbConnection`) |
| Driver SQL | `Microsoft.Data.SqlClient` 7.1.1 |
| Base de datos | SQL Server (probado contra una instancia local `SQLEXPRESS`) |
| Vistas | Razor Views (`.cshtml`), Bootstrap 5, jQuery Validation (unobtrusive) |
| Lenguaje | C# 13 / Nullable habilitado / `ImplicitUsings` |

## Arquitectura

La aplicación sigue MVC clásico, pero con una capa de **Repositorios** intermedia entre los controladores y la base de datos. Los controladores **no contienen SQL ni abren conexiones**: solo orquestan la petición HTTP, validan el modelo y delegan en el repositorio correspondiente.

```
Browser
   │  HTTP (GET / POST)
   ▼
Controllers/*Controller.cs        ← valida ModelState, maneja TempData, PRG
   │  llama a interfaces inyectadas por DI
   ▼
Repositorios/I*Repositorio.cs     ← contrato (testeable / sustituible)
Repositorios/*Repositorio.cs      ← implementación con Dapper
   │  QueryAsync / ExecuteAsync / ExecuteScalarAsync
   │  commandType: CommandType.StoredProcedure
   ▼
SQL Server — BibliotecaDB
   └─ Procedimientos almacenados (sp_*)  ← única puerta de entrada a las tablas
```

Cada repositorio abre su propia `SqlConnection` de corta duración (patrón *connection-per-operation*, típico de Dapper) usando la cadena de conexión leída desde `IConfiguration`. Las operaciones que modifican más de una tabla (crear un préstamo, registrar una devolución) usan una **transacción explícita** (`SqlTransaction`) para garantizar atomicidad entre el encabezado del préstamo, el detalle y el stock de ejemplares.

## Estructura del repositorio

> El repositorio Git tiene su raíz en `lab-semana08/` (donde está `lab-semana08.slnx`). Los scripts SQL viven un nivel arriba, en `Semana8/Database/`, y **no están versionados en este repo** — se ejecutan manualmente contra SQL Server antes de correr la aplicación.

```
Semana8/
├── Database/                          # scripts SQL (fuera del repo Git de la app)
│   ├── 01_Schema.sql                   # CREATE DATABASE + tablas + constraints
│   ├── 02_Seed.sql                     # datos de prueba (autores, libros, socios, préstamos)
│   └── 03_StoredProcedures.sql         # todos los procedimientos almacenados (sp_*)
│
└── lab-semana08/                       # ← raíz del repositorio Git / de la solución
    ├── lab-semana08.slnx                # archivo de solución (.NET)
    ├── README.md                        # este archivo
    └── lab-semana08/                    # proyecto ASP.NET Core Web App (MVC)
        ├── Program.cs                    # bootstrap, DI de repositorios, pipeline HTTP
        ├── appsettings.json              # cadena de conexión "BibliotecaDB"
        ├── lab-semana08.csproj           # TargetFramework net10.0, paquetes NuGet
        │
        ├── Models/                       # POCOs + DataAnnotations
        │   ├── Libro.cs
        │   ├── Autor.cs
        │   ├── Socio.cs
        │   ├── PrestamoReporte.cs         # fila del reporte de préstamos (por libro)
        │   ├── PrestamoCrearViewModel.cs  # ViewModel del formulario "Nuevo préstamo"
        │   └── ErrorViewModel.cs
        │
        ├── Repositorios/                 # acceso a datos con Dapper
        │   ├── ILibroRepositorio.cs       / LibroRepositorio.cs
        │   ├── ISocioRepositorio.cs       / SocioRepositorio.cs
        │   ├── IAutorRepositorio.cs       / AutorRepositorio.cs
        │   └── IPrestamoRepositorio.cs    / PrestamoRepositorio.cs
        │
        ├── Controllers/
        │   ├── HomeController.cs          # dashboard con estadísticas en vivo
        │   ├── LibrosController.cs        # CRUD + búsqueda por título
        │   ├── SociosController.cs        # listado + alta (valida DNI duplicado)
        │   └── PrestamosController.cs     # crear préstamo, reporte, registrar devolución
        │
        ├── Views/
        │   ├── Home/Index.cshtml          # landing page con hero + tarjetas + stats
        │   ├── Libros/                    # Index, Details, Create, Edit, Delete, _LibroRow (partial)
        │   ├── Socios/                    # Index, Create
        │   ├── Prestamos/                 # Create (nuevo préstamo), Reporte (filtro + devolución)
        │   └── Shared/
        │       ├── _Layout.cshtml         # layout + menú de navegación
        │       └── _ValidationScriptsPartial.cshtml
        │
        └── wwwroot/                       # Bootstrap, jQuery, jQuery Validation, CSS propio
```

## Modelo de base de datos

`BibliotecaDB` tiene cinco tablas relacionadas así:

```
Autores (1) ──── (N) Libros (1) ──── (N) DetallePrestamo (N) ──── (1) Prestamos (N) ──── (1) Socios
```

| Tabla | Columnas clave | Notas |
|---|---|---|
| `Autores` | `AutorId` PK, `Nombre`, `Nacionalidad`, `Activo` | Alimenta el combo de autores en Libros |
| `Libros` | `LibroId` PK, `Titulo`, `ISBN` (único), `AutorId` FK, `Ejemplares`, `Activo` | `Ejemplares` representa el stock disponible; se descuenta al prestar y se repone al devolver. Baja lógica vía `Activo = 0` |
| `Socios` | `SocioId` PK, `DNI` (único), `Nombre`, `Email`, `Activo` | El DNI se valida como único antes del INSERT |
| `Prestamos` | `PrestamoId` PK, `SocioId` FK, `FechaPrestamo`, `FechaLimite`, `Estado` (`Pendiente`/`Devuelto`, con `CHECK`) | Es la "cabecera": un préstamo puede incluir varios libros |
| `DetallePrestamo` | PK compuesta (`PrestamoId`, `LibroId`), `FechaDevolucion` (nullable) | Una fila por libro prestado dentro de un préstamo; `FechaDevolucion IS NULL` ⇒ ese libro sigue pendiente |

El estado mostrado en el **reporte de préstamos** es **por línea de libro** (no por cabecera): se calcula con un `CASE` sobre `DetallePrestamo.FechaDevolucion`, porque un mismo préstamo puede tener algunos libros ya devueltos y otros todavía pendientes. El `Estado` de la cabecera (`Prestamos.Estado`) solo pasa a `Devuelto` cuando **todas** sus líneas tienen `FechaDevolucion` no nula.

## Procedimientos almacenados

Todos viven en `Database/03_StoredProcedures.sql` y son el único punto de acceso a las tablas (ningún repositorio hace `SELECT`/`INSERT`/`UPDATE` directo).

**Libros**
| Procedimiento | Uso |
|---|---|
| `sp_Libros_ListarActivos` | Listado con `INNER JOIN` a `Autores` (nombre del autor) |
| `sp_Libros_BuscarPorTitulo` | Búsqueda parcial (`LIKE`) por título, solo activos |
| `sp_Libros_ObtenerPorId` | Detalle / precarga de formularios Edit |
| `sp_Libros_Insertar` | Alta (devuelve el `LibroId` generado) |
| `sp_Libros_Actualizar` | Edición |
| `sp_Libros_Eliminar` | Baja lógica (`Activo = 0`) |
| `sp_Libros_DecrementarEjemplares` | Descuenta 1 ejemplar al crear un préstamo |
| `sp_Libros_IncrementarEjemplares` | Repone 1 ejemplar al registrar una devolución |

**Autores**
| Procedimiento | Uso |
|---|---|
| `sp_Autores_ListarActivos` | Combo desplegable en Create/Edit de Libros y en Nuevo préstamo |

**Socios**
| Procedimiento | Uso |
|---|---|
| `sp_Socios_ListarActivos` | Listado y combo del formulario de préstamo |
| `sp_Socios_ObtenerPorDNI` | Verifica duplicados antes del alta |
| `sp_Socios_Insertar` | Alta |

**Préstamos**
| Procedimiento | Uso |
|---|---|
| `sp_Prestamos_ReportePorFechas` | Reporte con `INNER JOIN` entre `Prestamos`, `DetallePrestamo`, `Libros` y `Socios`, filtrado por rango de fechas, estado por línea |
| `sp_Prestamos_Insertar` | Crea la cabecera del préstamo (`Estado = 'Pendiente'`) |
| `sp_DetallePrestamo_Insertar` | Agrega una línea (un libro) al préstamo |
| `sp_Prestamos_ContarPendientesPorSocio` | Cuenta libros pendientes de devolución de un socio (regla de máximo 3) |
| `sp_Prestamos_ContarPendientesTotal` | Total de préstamos pendientes, para el dashboard |
| `sp_DetallePrestamo_MarcarDevuelto` | Marca `FechaDevolucion` de una línea específica |
| `sp_Prestamos_ContarPendientesPorPrestamo` | Verifica si quedan líneas pendientes en un préstamo |
| `sp_Prestamos_ActualizarEstado` | Pasa la cabecera a `Devuelto` cuando ya no quedan líneas pendientes |

## Funcionalidades

### Libros (`LibrosController`)
- **Index**: listado de libros activos (autor incluido vía JOIN) con buscador por título.
- **Details**: ficha de un libro.
- **Create / Edit**: formulario con combo de autores, validado con DataAnnotations (`Required`, `StringLength`, `Range` en ejemplares).
- **Delete**: confirmación antes de la baja lógica.
- Fila de libro extraída a una **vista parcial** (`_LibroRow.cshtml`), reutilizada en el listado.

### Socios (`SociosController`)
- **Index**: listado de socios activos.
- **Create**: alta con validación de **DNI duplicado** vía `ModelState.AddModelError` (no deja que la base de datos falle por la restricción `UNIQUE`).

### Préstamos (`PrestamosController`)
- **Reporte** (`GET`): filtro por rango de fechas (`desde`/`hasta`, por querystring), conservado en `ViewData` para repoblar el formulario. Cada línea muestra socio, libro, fecha límite y estado (badge Pendiente/Devuelto).
- **Create** (`GET`/`POST`): registra un préstamo nuevo — selección de socio + checklist de libros con stock disponible (`Ejemplares > 0`). Al confirmar: crea la cabecera, una línea de detalle por libro seleccionado y descuenta el stock, todo en una sola transacción.
- **Devolver** (`POST`, desde la fila del reporte): marca la devolución de un libro puntual, repone su stock y, si era la última línea pendiente del préstamo, cierra la cabecera como `Devuelto`.

### Inicio (`HomeController`)
- Dashboard con tarjetas de acceso directo a Libros, Socios y Préstamos, y estadísticas en vivo (libros activos, socios activos, préstamos pendientes) leídas de los mismos repositorios.

## Reglas de negocio

- **Baja lógica siempre**: ningún `DELETE` físico; todo pasa por `Activo = 0` (Libros) o por el ciclo de estados de `Prestamos`/`DetallePrestamo`.
- **Máximo 3 libros pendientes por socio**: antes de crear un préstamo se cuentan los libros que ese socio tiene sin devolver (`DetallePrestamo.FechaDevolucion IS NULL`); si sumar los nuevos libros supera 3, se rechaza con un error de validación en el formulario.
- **Solo se pueden prestar libros con stock**: el formulario de "Nuevo préstamo" únicamente lista libros con `Ejemplares > 0`.
- **DNI único**: validado en la aplicación antes del INSERT, además de la restricción `UNIQUE` en la base de datos (doble defensa).
- **Estado por línea vs. por cabecera**: el reporte refleja el estado real de cada libro; la cabecera del préstamo solo se cierra cuando **todas** sus líneas están devueltas.

## Puesta en marcha

### Prerrequisitos
- [.NET SDK 10](https://dotnet.microsoft.com/) (`dotnet --version` ⇒ `10.x`)
- SQL Server (local, Express o completo) accesible desde la máquina donde corre la app

### 1. Crear la base de datos y sus objetos

Ejecutar en orden contra la instancia de SQL Server (ejemplo con `sqlcmd`, usando el codepage UTF-8 para no corromper tildes):

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -f 65001 -i Database/01_Schema.sql
sqlcmd -S "localhost\SQLEXPRESS" -E -f 65001 -i Database/02_Seed.sql
sqlcmd -S "localhost\SQLEXPRESS" -E -f 65001 -i Database/03_StoredProcedures.sql
```

> El flag `-f 65001` es importante: sin él, `sqlcmd` puede interpretar el archivo con el codepage de la consola y corromper caracteres acentuados (`García` → `Garc��a`) en las columnas `NVARCHAR`.

### 2. Configurar la cadena de conexión

En `lab-semana08/lab-semana08/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "BibliotecaDB": "Server=localhost\\SQLEXPRESS;Database=BibliotecaDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Ajustar `Server` al nombre de la instancia real (por ejemplo `localhost` si es la instancia por defecto, o usar autenticación SQL en vez de `Trusted_Connection` si corresponde).

### 3. Ejecutar la aplicación

```bash
cd lab-semana08/lab-semana08
dotnet restore
dotnet run
```

La app queda disponible en la URL que indique la consola (por ejemplo `http://localhost:5299`). El menú superior da acceso a **Libros**, **Socios** y **Reporte de préstamos**; desde el inicio también se puede ir directo a "Nuevo préstamo".

## Flujo de una petición

Ejemplo concreto — crear un préstamo:

1. `GET /Prestamos/Create` → el controlador pide a `ISocioRepositorio` y `ILibroRepositorio` los socios y libros activos (con stock) y los pasa a la vista vía `ViewBag`.
2. El usuario marca un socio y uno o más libros y envía el formulario (`POST /Prestamos/Create`).
3. El controlador valida `ModelState`, verifica la regla de máximo 3 libros pendientes llamando a `IPrestamoRepositorio.ContarPendientesPorSocioAsync`, y si todo es válido llama a `CrearAsync`.
4. `PrestamoRepositorio.CrearAsync` abre una `SqlTransaction`, inserta la cabecera (`sp_Prestamos_Insertar`), inserta una línea por libro (`sp_DetallePrestamo_Insertar`) y descuenta stock (`sp_Libros_DecrementarEjemplares`) por cada uno, y confirma la transacción.
5. El controlador guarda un mensaje de éxito en `TempData` y redirige (`RedirectToAction`) al reporte — patrón **Post/Redirect/Get**.

## Explicación: recorrido completo de una petición (caso Editar libro)

Acción elegida: **editar un libro** (`Libros/Edit`). Se eligió esta acción porque es la que más capas y las tres bolsas de datos de ASP.NET Core (`ViewData`, `TempData` y `Model`) toca en un solo flujo: lee un recurso existente, lo muestra en un formulario, lo valida, lo actualiza y confirma el resultado tras una redirección.

### 1. Ruta

El enrutamiento usa la ruta convencional definida en `Program.cs`:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

- `GET /Libros/Edit/3` → `controller = Libros`, `action = Edit`, `id = 3` (parte de la URL).
- `POST /Libros/Edit/3` → misma URL; el formulario (`<form asp-action="Edit">`) envía por POST al mismo recurso, con `LibroId` también viajando como campo oculto del formulario además de como segmento de ruta.

### 2. Controlador

`Controllers/LibrosController.cs` expone dos acciones para `Edit`:

```csharp
// GET: carga el libro existente y la lista de autores para el combo
public async Task<IActionResult> Edit(int id)
{
    ViewData["Title"] = "Editar libro";

    var libro = await _libroRepositorio.ObtenerPorIdAsync(id);
    if (libro is null) return NotFound();

    await CargarAutoresAsync();   // llena ViewBag.Autores
    return View(libro);           // libro es el Model de la vista
}

// POST: valida y persiste los cambios
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Libro libro)
{
    ViewData["Title"] = "Editar libro";

    if (id != libro.LibroId) return NotFound();

    if (!ModelState.IsValid)
    {
        await CargarAutoresAsync();
        return View(libro);       // re-muestra el formulario con los errores
    }

    await _libroRepositorio.ActualizarAsync(libro);
    TempData["Mensaje"] = $"Libro \"{libro.Titulo}\" actualizado correctamente.";
    return RedirectToAction(nameof(Index));   // PRG: termina en un GET nuevo
}
```

El controlador **no contiene SQL ni abre conexiones**: solo valida `ModelState` y delega en el repositorio inyectado por constructor (`ILibroRepositorio`, `IAutorRepositorio`).

### 3. Repositorio

`Repositorios/LibroRepositorio.cs` (implementa `ILibroRepositorio`) ejecuta Dapper contra procedimientos almacenados:

```csharp
public async Task<Libro?> ObtenerPorIdAsync(int libroId)
{
    using var conexion = CrearConexion();
    return await conexion.QueryFirstOrDefaultAsync<Libro>(
        "dbo.sp_Libros_ObtenerPorId",
        new { LibroId = libroId },
        commandType: CommandType.StoredProcedure);
}

public async Task ActualizarAsync(Libro libro)
{
    using var conexion = CrearConexion();
    await conexion.ExecuteAsync(
        "dbo.sp_Libros_Actualizar",
        new { libro.LibroId, libro.Titulo, libro.ISBN, libro.AutorId, libro.Ejemplares },
        commandType: CommandType.StoredProcedure);
}
```

Para el combo de autores, `AutorRepositorio.ListarActivosAsync()` llama a `dbo.sp_Autores_ListarActivos`.

### 4. Procedimiento almacenado

- `sp_Libros_ObtenerPorId` — `SELECT` con `INNER JOIN` a `Autores`, filtrado por `LibroId` (usado en el `GET`).
- `sp_Autores_ListarActivos` — `SELECT` de autores con `Activo = 1`, orden por nombre (alimenta el combo).
- `sp_Libros_Actualizar` — `UPDATE dbo.Libros SET Titulo=…, ISBN=…, AutorId=…, Ejemplares=… WHERE LibroId=@LibroId` (usado en el `POST`).

### 5. Vista

`Views/Libros/Edit.cshtml`, fuertemente tipada:

```cshtml
@model lab_semana08.Models.Libro
...
<input type="hidden" asp-for="LibroId" />
<input asp-for="Titulo" class="form-control" />
<select asp-for="AutorId" asp-items="ViewBag.Autores as SelectList">...</select>
<input asp-for="Ejemplares" class="form-control" />
```

Los `asp-for` enlazan directamente a las propiedades del `Model` (`Libro`), lo que también activa los `asp-validation-for` ligados a las `DataAnnotations` de esa misma clase (`[Required]`, `[StringLength]`, `[Range]`).

### 6. Paso de datos a la vista: ¿`ViewData`, `TempData` o `Model`?

En este único flujo se usan los tres, cada uno para algo distinto — no son intercambiables.

El **Model** (`@model Libro`) transporta el libro completo: `LibroId`, `Titulo`, `ISBN`, `AutorId`, `Ejemplares`. Se usa porque es el **recurso** que la vista tiene que mostrar, editar y volver a enviar por POST. Al ser fuertemente tipado, los `asp-for`/`asp-validation-for` quedan ligados en tiempo de compilación a las `DataAnnotations` de `Libro`, y el *model binder* reconstruye el objeto automáticamente desde los campos del formulario. Es la única opción cuando el dato **es** la entidad de negocio que se va a validar y persistir.

`ViewData["Title"]` transporta el título de la página ("Editar libro"). Se usa `ViewData` y no el `Model` porque es un dato de **presentación**, no un campo de `Libro`: no tiene sentido agregarlo a la entidad solo para mostrarlo en el `<title>`/`<h1>`. `ViewData` vive únicamente durante el ciclo petición→respuesta actual, que es exactamente lo que se necesita (se fija en el controlador y se lee una sola vez en la vista/layout).

`ViewBag.Autores` (azúcar sintáctica sobre el mismo diccionario que `ViewData`) transporta la lista de autores activos (`SelectList`) para el `<select>`. Tampoco va en el `Model` porque es información **auxiliar de UI** (las opciones del combo), no una propiedad de `Libro` — el libro solo guarda `AutorId`, no la lista completa de autores posibles. Igual que `ViewData`, solo necesita existir durante esta petición, así que no se justifica forzarla dentro del modelo ni usar `TempData` (que persistiría más de lo necesario).

`TempData["Mensaje"]` transporta el mensaje de confirmación ("Libro actualizado correctamente"). Aquí no sirven ni `Model` ni `ViewData`: el `POST` termina con `RedirectToAction(nameof(Index))` (patrón **Post/Redirect/Get**), lo que genera una **segunda petición HTTP** (el `GET /Libros/Index` que ejecuta el navegador al seguir la redirección). `ViewData`/`ViewBag` se destruyen al terminar la petición que los creó, así que **no sobreviven a un redirect**, y el `Model` tampoco — cada acción construye el suyo desde cero. `TempData` existe justamente para ese caso: se guarda (por defecto en cookie o sesión), sobrevive una redirección, se lee en la vista `Index` y se borra automáticamente después de leerse. Es la única de las tres opciones que podía usarse aquí.

En resumen: `Model` para el dato de dominio que se valida/persiste, `ViewData`/`ViewBag` para datos de presentación de un solo uso dentro de la misma petición, y `TempData` cuando el dato tiene que cruzar una redirección (siempre que se siga el patrón Post/Redirect/Get, como ocurre en **todas** las acciones de escritura de este proyecto: Libros, Socios y Préstamos).

## Notas de diseño

- **Todo async**: cada acción que toca datos es `async Task<IActionResult>`, y los repositorios usan `QueryAsync`/`ExecuteAsync`/`ExecuteScalarAsync` de Dapper. No hay `.Result` ni `.Wait()` en ninguna parte del código.
- **Sin SQL en controladores**: los controladores solo conocen interfaces (`ILibroRepositorio`, `ISocioRepositorio`, `IAutorRepositorio`, `IPrestamoRepositorio`), inyectadas por constructor y registradas en `Program.cs` con `AddScoped`.
- **TempData para feedback**: todas las operaciones de escritura (crear, editar, eliminar libro; crear socio; crear préstamo; devolver libro) muestran un mensaje de confirmación tras la redirección, en vez de renderizar directamente la vista del resultado.
- **Vista parcial reutilizable**: `_LibroRow.cshtml` evita duplicar el markup de una fila de libro entre el listado normal y los resultados de búsqueda.
- **CSS propio mínimo**: el rediseño del Home (hero con gradiente, tarjetas con hover, iconos SVG inline estilo *Feather*) vive en `wwwroot/css/site.css`, sin dependencias externas nuevas — todo se sirve desde `wwwroot/lib` (Bootstrap, jQuery, jQuery Validation) ya incluido en el proyecto.

## Observaciones y conclusiones

### Observaciones

1. **Separar el acceso a datos en repositorios con Dapper mantuvo los controladores libres de SQL.** Todas las acciones de `LibrosController`, `SociosController` y `PrestamosController` delegan en interfaces inyectadas (`ILibroRepositorio`, `ISocioRepositorio`, `IAutorRepositorio`, `IPrestamoRepositorio`), cumpliendo la separación de capas exigida sin que el controlador conozca ni abra una `SqlConnection`.

2. **La carga de datos de prueba evidenció un problema de codificación de caracteres.** Al ejecutar los scripts `.sql` con `sqlcmd` sin indicar el codepage, los nombres con tildes (por ejemplo "García Márquez") se insertaron corruptos en la base de datos; el problema se resolvió especificando `-f 65001` (UTF-8) al momento de ejecutar los scripts, lo que confirma que la codificación del archivo de origen y la del cliente SQL deben coincidir explícitamente.

3. **La regla de "máximo 3 libros pendientes por socio" ya estaba implícita en los datos semilla, pero no en el código.** El comentario del script de seed data mencionaba esta regla desde antes, pero no existía ninguna validación que la hiciera cumplir hasta que se implementó explícitamente en el flujo de creación de préstamos.

4. **Modelar el estado de un préstamo por línea (libro) en vez de por cabecera resultó más fiel a la realidad del negocio.** Un mismo préstamo puede tener algunos libros devueltos y otros pendientes simultáneamente; calcular el estado a partir de `DetallePrestamo.FechaDevolucion` en lugar de usar un único campo `Estado` en la cabecera fue necesario para que el reporte y la devolución funcionaran correctamente.

5. **Las operaciones que afectan varias tablas requirieron transacciones explícitas.** Tanto crear un préstamo (inserta en `Prestamos`, inserta en `DetallePrestamo` y descuenta `Ejemplares`) como devolver un libro (actualiza `DetallePrestamo`, repone `Ejemplares` y posiblemente actualiza `Prestamos`) solo garantizan consistencia si todas las operaciones se confirman o revierten juntas mediante `SqlTransaction`.

### Conclusiones

1. **Dapper con procedimientos almacenados da control fino sobre el SQL ejecutado, a costa de más código repetitivo.** A diferencia de un ORM completo, cada repositorio debe escribir explícitamente el mapeo de parámetros y el tipo de comando, pero a cambio se obtiene total transparencia sobre qué consulta se ejecuta y mejor rendimiento en operaciones puntuales.

2. **El patrón Post/Redirect/Get combinado con `TempData` es indispensable para una buena experiencia de usuario en aplicaciones MVC.** Evita el reenvío accidental de formularios al recargar la página y permite mostrar mensajes de confirmación que de otra forma se perderían, ya que `ViewData`/`ViewBag` no sobreviven una redirección.

3. **Las reglas de negocio no triviales no pueden delegarse completamente a las restricciones de la base de datos.** Un `CHECK` o una llave única cubren invariantes simples (como el DNI), pero reglas como el límite de préstamos pendientes requieren lógica explícita en la capa de aplicación (controlador/repositorio) que consulte el estado actual antes de decidir.

4. **Detalles de bajo nivel como la codificación de caracteres y la eliminación lógica son críticos para la integridad de los datos, aunque pasen desapercibidos al diseñar la arquitectura.** Un error de codepage al cargar datos o un `DELETE` físico en vez de lógico pueden comprometer la trazabilidad y la consistencia del sistema sin que el código de la aplicación en sí tenga ningún error.

5. **Documentar el recorrido completo de una petición (ruta → controlador → repositorio → procedimiento almacenado → vista) es una herramienta útil para verificar que la arquitectura en capas se respeta de extremo a extremo.** Este ejercicio permitió confirmar que ninguna capa se salta funciones de otra (por ejemplo, que la vista no accede directamente a datos) y facilita el mantenimiento y la incorporación de nuevas funcionalidades al proyecto.
