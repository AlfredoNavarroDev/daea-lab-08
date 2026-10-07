-- BibliotecaDB - procedimientos almacenados (Semana 08, MVC + Dapper)
USE BibliotecaDB;
GO

-- =========================================================
-- LIBROS
-- =========================================================

IF OBJECT_ID(N'dbo.sp_Libros_ListarActivos', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Libros_ListarActivos;
GO
CREATE PROCEDURE dbo.sp_Libros_ListarActivos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  l.LibroId,
            l.Titulo,
            l.ISBN,
            l.AutorId,
            a.Nombre AS AutorNombre,
            l.Ejemplares,
            l.Activo
    FROM dbo.Libros l
    INNER JOIN dbo.Autores a ON a.AutorId = l.AutorId
    WHERE l.Activo = 1
    ORDER BY l.Titulo;
END
GO

IF OBJECT_ID(N'dbo.sp_Libros_BuscarPorTitulo', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Libros_BuscarPorTitulo;
GO
CREATE PROCEDURE dbo.sp_Libros_BuscarPorTitulo
    @Titulo NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  l.LibroId,
            l.Titulo,
            l.ISBN,
            l.AutorId,
            a.Nombre AS AutorNombre,
            l.Ejemplares,
            l.Activo
    FROM dbo.Libros l
    INNER JOIN dbo.Autores a ON a.AutorId = l.AutorId
    WHERE l.Activo = 1
      AND l.Titulo LIKE '%' + @Titulo + '%'
    ORDER BY l.Titulo;
END
GO

IF OBJECT_ID(N'dbo.sp_Libros_ObtenerPorId', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Libros_ObtenerPorId;
GO
CREATE PROCEDURE dbo.sp_Libros_ObtenerPorId
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  l.LibroId,
            l.Titulo,
            l.ISBN,
            l.AutorId,
            a.Nombre AS AutorNombre,
            l.Ejemplares,
            l.Activo
    FROM dbo.Libros l
    INNER JOIN dbo.Autores a ON a.AutorId = l.AutorId
    WHERE l.LibroId = @LibroId;
END
GO

IF OBJECT_ID(N'dbo.sp_Libros_Insertar', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Libros_Insertar;
GO
CREATE PROCEDURE dbo.sp_Libros_Insertar
    @Titulo     NVARCHAR(200),
    @ISBN       NVARCHAR(20),
    @AutorId    INT,
    @Ejemplares INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Libros (Titulo, ISBN, AutorId, Ejemplares, Activo)
    VALUES (@Titulo, @ISBN, @AutorId, @Ejemplares, 1);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS LibroId;
END
GO

IF OBJECT_ID(N'dbo.sp_Libros_Actualizar', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Libros_Actualizar;
GO
CREATE PROCEDURE dbo.sp_Libros_Actualizar
    @LibroId    INT,
    @Titulo     NVARCHAR(200),
    @ISBN       NVARCHAR(20),
    @AutorId    INT,
    @Ejemplares INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Libros
    SET Titulo     = @Titulo,
        ISBN       = @ISBN,
        AutorId    = @AutorId,
        Ejemplares = @Ejemplares
    WHERE LibroId = @LibroId;
END
GO

IF OBJECT_ID(N'dbo.sp_Libros_Eliminar', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Libros_Eliminar;
GO
CREATE PROCEDURE dbo.sp_Libros_Eliminar
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- eliminacion logica: nunca DELETE fisico
    UPDATE dbo.Libros
    SET Activo = 0
    WHERE LibroId = @LibroId;
END
GO

-- =========================================================
-- AUTORES
-- =========================================================

IF OBJECT_ID(N'dbo.sp_Autores_ListarActivos', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Autores_ListarActivos;
GO
CREATE PROCEDURE dbo.sp_Autores_ListarActivos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AutorId, Nombre, Nacionalidad, Activo
    FROM dbo.Autores
    WHERE Activo = 1
    ORDER BY Nombre;
END
GO

-- =========================================================
-- SOCIOS
-- =========================================================

IF OBJECT_ID(N'dbo.sp_Socios_ListarActivos', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Socios_ListarActivos;
GO
CREATE PROCEDURE dbo.sp_Socios_ListarActivos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SocioId, DNI, Nombre, Email, Activo
    FROM dbo.Socios
    WHERE Activo = 1
    ORDER BY Nombre;
END
GO

IF OBJECT_ID(N'dbo.sp_Socios_ObtenerPorDNI', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Socios_ObtenerPorDNI;
GO
CREATE PROCEDURE dbo.sp_Socios_ObtenerPorDNI
    @DNI NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SocioId, DNI, Nombre, Email, Activo
    FROM dbo.Socios
    WHERE DNI = @DNI;
END
GO

IF OBJECT_ID(N'dbo.sp_Socios_Insertar', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Socios_Insertar;
GO
CREATE PROCEDURE dbo.sp_Socios_Insertar
    @DNI    NVARCHAR(15),
    @Nombre NVARCHAR(150),
    @Email  NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Socios (DNI, Nombre, Email, Activo)
    VALUES (@DNI, @Nombre, @Email, 1);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS SocioId;
END
GO

-- =========================================================
-- PRESTAMOS - registrar nuevo prestamo
-- =========================================================

IF OBJECT_ID(N'dbo.sp_Prestamos_Insertar', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Prestamos_Insertar;
GO
CREATE PROCEDURE dbo.sp_Prestamos_Insertar
    @SocioId       INT,
    @FechaPrestamo DATETIME2,
    @FechaLimite   DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
    VALUES (@SocioId, @FechaPrestamo, @FechaLimite, N'Pendiente');

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS PrestamoId;
END
GO

IF OBJECT_ID(N'dbo.sp_DetallePrestamo_Insertar', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DetallePrestamo_Insertar;
GO
CREATE PROCEDURE dbo.sp_DetallePrestamo_Insertar
    @PrestamoId INT,
    @LibroId    INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion)
    VALUES (@PrestamoId, @LibroId, NULL);
END
GO

IF OBJECT_ID(N'dbo.sp_Libros_DecrementarEjemplares', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Libros_DecrementarEjemplares;
GO
CREATE PROCEDURE dbo.sp_Libros_DecrementarEjemplares
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Libros
    SET Ejemplares = Ejemplares - 1
    WHERE LibroId = @LibroId AND Ejemplares > 0;
END
GO

IF OBJECT_ID(N'dbo.sp_Prestamos_ContarPendientesPorSocio', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Prestamos_ContarPendientesPorSocio;
GO
CREATE PROCEDURE dbo.sp_Prestamos_ContarPendientesPorSocio
    @SocioId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- cuenta libros pendientes de devolucion del socio (no cabeceras de prestamo)
    SELECT COUNT(*)
    FROM dbo.DetallePrestamo dp
    INNER JOIN dbo.Prestamos p ON p.PrestamoId = dp.PrestamoId
    WHERE p.SocioId = @SocioId
      AND dp.FechaDevolucion IS NULL;
END
GO

IF OBJECT_ID(N'dbo.sp_Prestamos_ContarPendientesTotal', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Prestamos_ContarPendientesTotal;
GO
CREATE PROCEDURE dbo.sp_Prestamos_ContarPendientesTotal
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM dbo.Prestamos WHERE Estado = N'Pendiente';
END
GO

-- =========================================================
-- PRESTAMOS - reporte por intervalo de fechas
-- =========================================================

IF OBJECT_ID(N'dbo.sp_Prestamos_ReportePorFechas', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Prestamos_ReportePorFechas;
GO
CREATE PROCEDURE dbo.sp_Prestamos_ReportePorFechas
    @Desde DATETIME2,
    @Hasta DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    -- Estado es por linea (libro): Pendiente si no tiene FechaDevolucion, Devuelto si ya la tiene.
    SELECT  p.PrestamoId,
            l.LibroId,
            s.Nombre      AS SocioNombre,
            l.Titulo      AS LibroTitulo,
            p.FechaPrestamo,
            p.FechaLimite,
            dp.FechaDevolucion,
            CASE WHEN dp.FechaDevolucion IS NULL THEN N'Pendiente' ELSE N'Devuelto' END AS Estado
    FROM dbo.Prestamos p
    INNER JOIN dbo.Socios s           ON s.SocioId = p.SocioId
    INNER JOIN dbo.DetallePrestamo dp ON dp.PrestamoId = p.PrestamoId
    INNER JOIN dbo.Libros l           ON l.LibroId = dp.LibroId
    WHERE p.FechaPrestamo >= @Desde
      AND p.FechaPrestamo <= @Hasta
    ORDER BY p.FechaPrestamo, s.Nombre;
END
GO

-- =========================================================
-- PRESTAMOS - registrar devolucion de un libro
-- =========================================================

IF OBJECT_ID(N'dbo.sp_DetallePrestamo_MarcarDevuelto', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DetallePrestamo_MarcarDevuelto;
GO
CREATE PROCEDURE dbo.sp_DetallePrestamo_MarcarDevuelto
    @PrestamoId INT,
    @LibroId    INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.DetallePrestamo
    SET FechaDevolucion = SYSDATETIME()
    WHERE PrestamoId = @PrestamoId
      AND LibroId = @LibroId
      AND FechaDevolucion IS NULL;
END
GO

IF OBJECT_ID(N'dbo.sp_Libros_IncrementarEjemplares', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Libros_IncrementarEjemplares;
GO
CREATE PROCEDURE dbo.sp_Libros_IncrementarEjemplares
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Libros
    SET Ejemplares = Ejemplares + 1
    WHERE LibroId = @LibroId;
END
GO

IF OBJECT_ID(N'dbo.sp_Prestamos_ContarPendientesPorPrestamo', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Prestamos_ContarPendientesPorPrestamo;
GO
CREATE PROCEDURE dbo.sp_Prestamos_ContarPendientesPorPrestamo
    @PrestamoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM dbo.DetallePrestamo WHERE PrestamoId = @PrestamoId AND FechaDevolucion IS NULL;
END
GO

IF OBJECT_ID(N'dbo.sp_Prestamos_ActualizarEstado', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Prestamos_ActualizarEstado;
GO
CREATE PROCEDURE dbo.sp_Prestamos_ActualizarEstado
    @PrestamoId INT,
    @Estado     NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Prestamos
    SET Estado = @Estado
    WHERE PrestamoId = @PrestamoId;
END
GO
