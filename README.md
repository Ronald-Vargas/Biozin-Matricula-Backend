# Biozin Matrícula — Backend

API REST para el sistema de gestión de matrículas académicas de Biozin. Permite administrar estudiantes, profesores, cursos, carreras, ofertas académicas y pagos.

## Tecnologías

- **.NET 8.0** + ASP.NET Core
- **Entity Framework Core 8** con SQL Server
- **JWT Bearer** para autenticación
- **BCrypt** para encriptación de contraseñas
- **MailKit** para envío de correos
- **AutoMapper** para mapeo de entidades
- **Swagger** para documentación de la API

## Arquitectura

El proyecto sigue una arquitectura de capas:

```
Biozin-Matricula.API           → Controladores, configuración, plantillas de email
Biozin-Matricula.LogicaNegocio → Reglas de negocio
Biozin-Matricula.AccesoDatos   → Acceso a base de datos (EF Core, Unit of Work)
Biozin-Matricula.Dominio       → Entidades, DTOs, interfaces
Biozin-Matricula.Utilidades    → Helpers comunes
```

## Requisitos

- .NET 8 SDK
- SQL Server (local o remoto)
- Cuenta SMTP (el proyecto usa Mailtrap por defecto)

## Configuración

Edita `Biozin-Matricula.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=db_matricula;..."
  },
  "Jwt": {
    "Key": "...",
    "Issuer": "BiozinMatriculaAPI",
    "Audience": "BiozinMatriculaCliente"
  },
  "EmailSettings": {
    "Host": "...",
    "Port": 587,
    "Username": "...",
    "Password": "..."
  }
}
```

## Ejecutar el proyecto

```bash
cd Biozin-Matricula.API
dotnet restore
dotnet run
```

La API queda disponible en `https://localhost:7xxx`. Swagger en `/swagger`.

## Módulos

| Módulo | Descripción |
|--------|-------------|
| **Auth** | Login, recuperación y cambio de contraseña temporal |
| **Portal Estudiante** | Perfil, ofertas disponibles, matrícula, historial, pagos |
| **Estudiantes** | CRUD de estudiantes |
| **Profesores** | CRUD de profesores |
| **Administradores** | CRUD de administradores y actividad reciente |
| **Carreras** | CRUD de carreras académicas |
| **Cursos** | CRUD de cursos con prerequisitos y precios |
| **Asignaciones** | Relación carrera-curso |
| **Periodos** | Gestión de periodos académicos |
| **Oferta Académica** | Apertura de cursos por periodo, aula y profesor |
| **Aulas** | CRUD de aulas |
| **Ajustes** | Configuración general del sistema |

## Autenticación y roles

Los roles se determinan automáticamente por el dominio del correo institucional:

| Dominio | Rol |
|---------|-----|
| `@est.biozin.edu.cr` | Estudiante |
| `@prof.biozin.edu.cr` | Profesor |
| `@admin.biozin.edu.cr` | Administrador |

El login devuelve un JWT con duración de 8 horas. Los endpoints del portal de estudiante requieren rol `Estudiante`; los de administración requieren rol `Administrador`.

## Flujo de matrícula

1. El administrador crea el periodo, las ofertas académicas y habilita las fechas de matrícula.
2. El estudiante inicia sesión y visualiza las ofertas disponibles para su carrera.
3. El sistema valida prerequisitos, choques de horario y si el curso ya fue matriculado.
4. Al matricular se genera automáticamente un pago y se envía un comprobante por correo.

## Correos automáticos

El sistema envía correos en los siguientes eventos:

- Registro de estudiante (credenciales de acceso)
- Registro de profesor/administrador (credenciales de acceso)
- Matrícula exitosa (comprobante)
- Pago realizado (comprobante)
- Solicitud de recuperación de contraseña (código)
