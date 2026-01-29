# Zurich API

Backend del sistema Zurich desarrollado con ASP.NET Core y SQL Server.

Este proyecto expone una API REST que maneja autenticación, roles, permisos, gestión de clientes, pólizas y perfil del usuario, consumida por el frontend Zurich Web.

---

## Tecnologías

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core
- SQL Server
- Autenticación JWT
- Redis (manejo de refresh tokens)
- Arquitectura por capas (Controller, Repository, DataAccess)
- LINQ
- C#

---

## Requisitos

Antes de ejecutar el proyecto asegúrate de tener instalado:

- .NET SDK 8
- SQL Server (LocalDB o instancia local)
- Visual Studio 2022 o superior
- Git
- Redis (opcional para entorno local, recomendado)

---

## Rama de trabajo

La rama que debe utilizarse para trabajar en local es:

```text
development

```

### Clonar el repositorio 
 ```text
git clone https://github.com/fersanti2896/ZurichAPI.git
git checkout development
```


### Configuración de variables de entorno
Para obtener la url productiva favor de enviar un correo a fersanti2896@gmail.com
 ```text
appsettings.json
```

### Credenciales
Administrador

 ```text
Email: fersanti2896@gmail.com
Password: Fersa169*
```

El administrador puede:
- Gestionar clientes
- Administrar pólizas
- Aprobar solicitudes de cancelación de pólizas
- Editar información de un cliente


Cliente

 ```text
Email: wendys@gmail.com
Password: Fersa169*
```

El cliente puede:
- Visualizar sus pólizas
- Solicitar cancelaciones
- Editar su información personal (dirección y teléfono)
