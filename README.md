# Siecha: A 3D Colombian Roguelike

> 🎮 **Siecha** es un juego de acción y combate en tercera persona desarrollado en Unity, con combates cuerpo a cuerpo y a distancia, cambio de personaje, IA de enemigos, sistema de stamina/guard-break, navegación con NavMesh, y un control general de escenas, UI, VFX, etc.
<table>
  <tr>
    <td><img src="Docs/Gameplay1.jpeg" alt="Screenshot 1" width="500"/></td>
    <td><img src="Docs/Cinematica1.png" alt="Screenshot 2" width="500"/></td>
  </tr>
  <tr>
    <td><img src="Docs/Interfaz1.png" alt="Screenshot 3" width="500"/></td>
    <td><img src="Docs/Gameplay2.png" alt="Screenshot 4" width="500"/></td>
  </tr>
</table>


---

## 📄 Índice

- [Descripción general](#descripción-general)  
- [Funcionalidades principales](#funcionalidades-principales)  
- [Arquitectura del sistema](#arquitectura-del-sistema)  
- [Estructura del proyecto](#estructura-del-proyecto)  
- [Flujo principal del juego](#flujo-principal-del-juego)  
- [Patrones de diseño y decisiones técnicas](#patrones-de-diseño-y-decisiones-técnicas)
- [Integrantes](#Integrantes-del-Equipo)
- [Cómo empezar / Uso](#Requerimientos-Técnicos)
- [Planes futuros / Próximos pasos](#planes-futuros--próximos-pasos)  
- [Licencia](#licencia)  

---

## Descripción general

SAMI es un juego de acción-combate en tercera persona creado en Unity con las siguientes características principales:  

- Dos personajes jugables: **Muisca** y **Español**, cada uno con estilo de combate propio.  
- Capacidad de cambiar entre combate cuerpo a cuerpo y combate a distancia (melee / ranged).  
- IA enemiga con máquinas de estados y decisiones basadas en utility-AI.  
- Combate con sistema de stamina: bloquear consume stamina, lo que puede llevar a guard-break y permitir finalizadores (finishers).  
- Navegación mediante NavMesh tanto para jugador como para enemigos.  
- Uso de tecnologías en Unity como Input System (para entrada cruzada), Cinemachine (cámaras), DOTween (animaciones UI), y Universal Render Pipeline (URP). 

Este README sirve como punto de partida para entender la base de código, su organización, y cómo están estructurados los sistemas principales.  

---

## Funcionalidades principales

- Combate cuerpo a cuerpo (ataques ligeros, fuertes, combos, bloqueos, esquives).
- Combate a distancia con armas (arco, arcabuz, proyectiles).
- Cambio de personaje (Muisca ↔ Español). 
- Gestión de estadísticas, salud y stamina tanto de jugador como de enemigos. 
- IA de enemigos con lógica de decisión (utility-AI), máquinas de estado independientes, con estados proactivos y reactivos. 
- Navegación por NavMesh, con agentes de navegación para enemigos. 
- Sistemas de UI / menú / navegación, incluyendo menú principal, tutoriales, configuración, video playback, etc.
- Arquitectura de datos mediante ScriptableObjects para armas, estadísticas, configuración de personajes/enemigos.

---

## Arquitectura del sistema

### Domainos principales

| Dominio del sistema       | Propósito principal                                                                 | Componentes clave |
|--------------------------|--------------------------------------------------------------------------------------|------------------|
| Player Combat            | Gestiona ataques, combos, bloqueos, esquive, cambio de armas/personaje             | `ControladorCombate`, `CombatStateMachine`, `InputJugador` |
| Enemy AI                 | Controla comportamiento de enemigos, decisiones, combates                          | `Enemigo`, `MaquinaDeEstados`, `UtilityAI_Grupal`, `UtilityAI_Tactico` |
| Health & Damage          | Maneja salud, stamina, guard-break, stun para jugador y enemigos                   | `HealthComp`, `EstadisticasCombate`, `DetectorImpactoMelee`|
| Navigation               | Proporciona pathfinding y restricciones de movimiento                             | Unity NavMesh, `NavMeshAgent`, `ControladorMovimiento`|
| UI & Menús               | Manejo de menús, configuración, tutoriales, videos, navegación UI                | `ComboNavigator`, `VolumeSettings`, `VideoPlayer`, escenas de menú |

---

## Estructura del proyecto

El repositorio sigue la organización estándar de Unity, estructurando scripts por área funcional. 

Por ejemplo:  
- `Assets/Scripts/_Player/` — Todo lo relacionado con jugador (movimiento, combate, entrada, estados).  
- `Assets/Scripts/_Player/combat_Melee/` — Estados de combate melee, sistema de combos, detección de daño.  
- `Assets/Scripts/_Player/combat_Ranged/` — Lógica de armas a distancia (arco, arcabuz, proyectiles).  
- `Assets/Scripts/Enemy/` — Control de IA, scripts de enemigo, estados, salud.  
- `Assets/Scenes/` — Escenas del juego: menú, capítulos, demo de combate, etc.  
- `Assets/Scripts/ScriptableObjects/` — Datos serializados: armas, estadísticas de personajes y enemigos, configuración general.  

---

## Flujo principal del juego

[**DIAGRAMA DEL FLUJO DE JUEGO (Gameplay Loop)**]  
![DiagramaArquitectura](Docs/DiagramaArquitectura.png)

1. El jugador inicia desde la escena de introducción — video de inicialización.  
2. Luego pasa al menú principal, desde donde puede acceder a juego, configuración, tutoriales, etc.  
3. Al iniciar una escena de juego:  
   - `InputJugador` se inicializa y configura el mapa de acciones.  
   - `ControladorCambiarPersonaje` activa el personaje por defecto (Muisca).  
   - `ControladorCombate` carga estadísticas, equipa armas, inicializa la máquina de estados de combate.  
   - `EnemyManager` registra todos los enemigos en la escena.  
   - Cada enemigo inicia su propia máquina de estados, IA y navegación. 

4. Durante el combate: inputs del jugador disparan transiciones de estado → animaciones + colliders → detección de impacto → aplicación de daño → posible reacción enemiga → decisiones de IA → ataque enemigo → daño al jugador → y así en bucle hasta fin de combate.

---

## Patrones de diseño y decisiones técnicas

- **Singleton**: para sistemas globales como `InputJugador`, `EnemyManager`, `ControladorCambiarPersonaje`, `CameraShakeManager`, `GameDataManager`.
- **State Pattern (máquinas de estados)**:  
  - Para el combate del jugador: `CombatStateMachine` + clases derivadas de `CombatState`.  
  - Para IA enemiga: `MaquinaDeEstados` + `EstadoBase`, con transiciones normales e interrupciones globales (`DesdeCualquier`). 
- **Facade Pattern**: `ControladorCombate` actúa como fachada que simplifica el acceso a subsistemas complejos como estado de combate, gestión de armas, cálculo de daño, VFX, etc.
- **ScriptableObject for data**: para definir armas, estadísticas, configuración, lo que permite al diseñador editar valores sin modificar código. 
- **Observer / Event-Driven (para animaciones)**: se usan eventos de animación para activar colliders de armas, volver a estado idle, temporizadores para recuperación luego de guard-break, etc. 

---
## Integrantes del Equipo

| Integrante                 | Rol                                                   | GitHub                                 |
| -------------------------- | ----------------------------------------------------- | -------------------------------------- |
| **Ricardo López Murcia**   | Tester / Documentador                                 | [GitHub](https://github.com/[usuario]) |
| **Stefany López**          | Personajes 3D, animación, shaders, UI/UX              | [GitHub](https://github.com/[usuario]) |
| **Leidy Katerine Pinzón**  | Diseño de personajes, animación, shaders, cinemáticas | [GitHub](https://github.com/[usuario]) |
| **Gian Luis Osorio**       | Environments 3D                                       | [GitHub](https://github.com/[usuario]) |
| **Juliana María Deaza**    | Environments 3D                                       | [GitHub](https://github.com/[usuario]) |
| **Juan José Camacho**      | Programador de IA                                     | [GitHub](https://github.com/[usuario]) |
| **David José Gutiérrez**   | Guionista                                             | [GitHub](https://github.com/[usuario]) |
| **Nicolás Hurtado Vargas** | Programador principal / Mecánicas                     | [GitHub](https://github.com/[usuario]) |

---
## Requerimientos Técnicos

### Controles

* **Teclado y Mouse**: Totalmente compatible.
* **Mando / Gamepad**: Soportado (Xbox / PlayStation / genérico).

### Conectividad

* **No requiere conexión a Internet** para jugar.
* No necesita servicios en línea, cuentas externas ni autenticación.

### Sistema

Estos valores son aproximados basados en pruebas internas.

#### **Requisitos Mínimos**

* **SO:** Windows 10 (64 bits)
* **Procesador:** Intel Core i5 (6ª generación o equivalente)
* **GPU:** NVIDIA GTX 1050 / GTX 960 / equivalente
* **RAM:** 8 GB
* **Almacenamiento:** 1.5 GB de espacio disponible
* **DirectX:** Versión 11 o superior

#### **Requisitos Recomendados**

(Probado en equipo de desarrollo)

* **Procesador:** Intel Core i7 10ª generación
* **GPU:** NVIDIA GTX 1650
* **RAM:** 16 GB
* **Rendimiento esperado:** ~50 FPS 
* **Almacenamiento:** 1.5 GB de espacio disponible

### Otros

* No requiere instalación de software adicional.
* No usa DRM.

- 

### Instalación
1. Descargar el ultimo [Ejecutable](https://github.com/NikolasH03/Siecha/releases/latest).
2. Ejecutar el archivo Gameplay_Siecha.exe
3. ¡Juega!

## Descargar proyecto de Unity

1. Clona el repositorio.  
2. Abre el proyecto en Unity (versión compatible con URP + Input System + Cinemachine + DOTween).  
3. Carga la escena `Introduccion.unity`.  
4. Desde el menú puedes acceder a la escena de gameplay.
5. Si quieres probar o modificar datos (armas, estadísticas, enemigos), edita los correspondientes **ScriptableObjects** en `Assets/Scripts/ScriptableObjects/`.  

> ⚠️ Asegúrate de tener instalado **DOTween**, **Cinemachine**, y que tu proyecto esté configurado con **Universal Render Pipeline (URP)**.  

---

## Planes futuros / Próximos pasos

- Expandir árbol de habilidades y sistema de coleccionables.  
- Integrar animaciones UI y 3D para selección de habilidades, menú de inventario, etc.  
- Refinar sistema de combate y comportamiento de enemigos: esquives, parry, revivir, sistema de intentos.  
- Añadir más escenas, narrativa, cinemáticas para capítulos del juego completo.  
- Mejorar sistema de datos y configuración mediante ScriptableObjects para facilitar extensibilidad.  

---

## Licencia

Este proyecto está bajo la **Licencia MIT**:

```
MIT License

Copyright (c) [AÑO] [TU NOMBRE / EQUIPO]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
## Licencia de Assets y Propiedad Intelectual

Aunque el **código del proyecto está bajo Licencia MIT**, todos los **assets visuales, modelos 3D, animaciones, texturas, música, sonidos, narrativa, guion, cinemáticas, arte conceptual y cualquier otro contenido creativo asociado a *Siecha*** están protegidos por derechos de autor y **NO** están cubiertos por la Licencia MIT.

**No se permite** copiar, redistribuir, modificar, vender o reutilizar estos assets en otros proyectos sin permiso explícito del equipo desarrollador.

Si deseas usar algún asset o contenido creativo de *Siecha*, contáctanos para obtener autorización.


