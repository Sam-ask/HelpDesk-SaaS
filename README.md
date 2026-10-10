# HelpDesk Corporativo SaaS

Un sistema transaccional de gestión de incidencias (HelpDesk) de TI diseñado bajo un modelo de Software as a Service (SaaS). Esta plataforma optimiza el flujo de soporte técnico mediante enrutamiento SPA, paneles analíticos y una arquitectura segura con control de accesos basado en roles (RBAC).

---

## Características Principales

- Gestión del Ciclo de Vida del Ticket: Creación, asignación, semaforización por prioridad (Baja a Urgente) y categorización de problemas.
- Control de Accesos Basado en Roles (RBAC): Vistas y permisos dinámicos restringidos según el perfil del usuario (Administrador, Técnico, Empleado).
- Dashboard Analítico: Panel gerencial con métricas en tiempo real, incluyendo volumen por prioridad, distribución por categorías y tasa de resolución global.
- Auditoría de Personal: Monitoreo del rendimiento del equipo de soporte técnico con métricas de tickets asignados, en proceso y resueltos.
- Historial Inmutable: Línea de tiempo detallada que registra cada actualización de estado o reasignación de los tickets.
- Diseño UI/UX Premium: Interfaz limpia, asíncrona y fluida orientada a herramientas empresariales B2B.

---

## Stack Tecnológico

Frontend 
- Angular
- TypeScript
- HTML5 / CSS3
- Bootstrap 5 

Backend 
- C# .NET Core
- Entity Framework Core
- Arquitectura RESTful API

Base de Datos
- SQL Server

---

## Instalación y Despliegue Local

### Prerrequisitos
- [Node.js](https://nodejs.org/) y Angular CLI (npm install -g @angular/cli)
- [.NET SDK](https://dotnet.microsoft.com/download)
- SQL Server y SQL Server Management Studio (SSMS)

### Configuración del Backend (C# .NET)
1. Navega a la carpeta del backend: cd Backend-CSharp/HelpDeskAPI
2. Actualiza la cadena de conexión en appsettings.json con las credenciales de tu servidor SQL local.
3. Aplica las migraciones para generar la base de datos:
   dotnet ef database update
4. Inicia el servidor:
   dotnet run

### Configuración del Frontend (Angular)
1. Navega a la carpeta del frontend: cd Frontend-Angular
2. Instala las dependencias de Node:
   npm install
3. Levanta el entorno de desarrollo:
   ng serve
4. Abre tu navegador en http://localhost:4200

---

## Estructura de Roles de Prueba

Para probar el sistema localmente, puedes generar usuarios con los siguientes perfiles:
- Administrador: Acceso total (creación de usuarios, auditoría de técnicos, borrado de tickets).
- Técnico: Gestión de tickets asignados y actualización de estatus.
- Empleado: Creación de tickets propios y visualización de progreso.
