# SAMI

> 🎮 **SAMI** es un juego de acción y combate en tercera persona desarrollado en Unity, con combates cuerpo a cuerpo y a distancia, cambio de personaje, IA de enemigos, sistema de stamina/guard-break, navegación con NavMesh, y un control general de escenas, UI, VFX, etc.
![Exploración en Siecha](Docs/Gameplay1.jpeg)
![Combate en Siecha](Docs/Gameplay2.png)

---

## 📄 Índice

- [Descripción general](#descripción-general)  
- [Funcionalidades principales](#funcionalidades-principales)  
- [Arquitectura del sistema](#arquitectura-del-sistema)  
- [Estructura del proyecto](#estructura-del-proyecto)  
- [Flujo principal del juego](#flujo-principal-del-juego)  
- [Patrones de diseño y decisiones técnicas](#patrones-de-diseño-y-decisiones-técnicas)  
- [Cómo empezar / Uso](#cómo-empezar--uso)  
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
- Uso de tecnologías en Unity como Input System (para entrada cruzada), Cinemachine (cámaras), DOTween (animaciones UI), y Universal Render Pipeline (URP). :contentReference[oaicite:1]{index=1}

Este README sirve como punto de partida para entender la base de código, su organización, y cómo están estructurados los sistemas principales.  

---

## Funcionalidades principales

- Combate cuerpo a cuerpo (ataques ligeros, fuertes, combos, bloqueos, esquives).
- Combate a distancia con armas (arco, arcabuz, proyectiles).
- Cambio de personaje (Muisca ↔ Español). 
- Gestión de estadísticas, salud y stamina tanto de jugador como de enemigos. 
- IA de enemigos con lógica de decisión (utility-AI), máquinas de estado independientes, con estados proactivos y reactivos. {index=6}  
- Navegación por NavMesh, con agentes de navegación para personajes y enemigos. 
- Sistemas de UI / menú / navegación, incluyendo menú principal, tutoriales, configuración, video playback, etc.
- Arquitectura de datos mediante ScriptableObjects para armas, estadísticas, configuración de personajes/enemigos.

---

## Arquitectura del sistema

### Domainos principales

| Dominio del sistema       | Propósito principal                                                                 | Componentes clave |
|--------------------------|--------------------------------------------------------------------------------------|------------------|
| Player Combat            | Gestiona ataques, combos, bloqueos, esquive, cambio de armas/personaje             | `ControladorCombate`, `CombatStateMachine`, `InputJugador` :contentReference[oaicite:10]{index=10} |
| Enemy AI                 | Controla comportamiento de enemigos, decisiones, combates                          | `Enemigo`, `MaquinaDeEstados`, `UtilityAI_Grupal`, `UtilityAI_Tactico` :contentReference[oaicite:11]{index=11} |
| Health & Damage          | Maneja salud, stamina, guard-break, stun para jugador y enemigos                   | `HealthComp`, `EstadisticasCombate`, `DetectorImpactoMelee` :contentReference[oaicite:12]{index=12} |
| Navigation               | Proporciona pathfinding y restricciones de movimiento                             | Unity NavMesh, `NavMeshAgent`, `ControladorMovimiento` :contentReference[oaicite:13]{index=13} |
| UI & Menús               | Manejo de menús, configuración, tutoriales, videos, navegación UI                | `ComboNavigator`, `VolumeSettings`, `VideoPlayer`, escenas de menú :contentReference[oaicite:14]{index=14} |

---

## Estructura del proyecto

El repositorio sigue la organización estándar de Unity, estructurando scripts por área funcional. :contentReference[oaicite:15]{index=15}  

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

- **Singleton**: para sistemas globales como `InputJugador`, `EnemyManager`, `ControladorCambiarPersonaje`, `CameraShakeManager`, `GameDataManager`. :contentReference[oaicite:18]{index=18}  
- **State Pattern (máquinas de estados)**:  
  - Para el combate del jugador: `CombatStateMachine` + clases derivadas de `CombatState`.  
  - Para IA enemiga: `MaquinaDeEstados` + `EstadoBase`, con transiciones normales e interrupciones globales (`DesdeCualquier`). 
- **Facade Pattern**: `ControladorCombate` actúa como fachada que simplifica el acceso a subsistemas complejos como estado de combate, gestión de armas, cálculo de daño, VFX, etc.
- **ScriptableObject for data**: para definir armas, estadísticas, configuración, lo que permite al diseñador editar valores sin modificar código. 
- **Observer / Event-Driven (para animaciones)**: se usan eventos de animación para activar colliders de armas, volver a estado idle, temporizadores para recuperación luego de guard-break, etc. 

---

## Cómo empezar / Uso

1. Clona el repositorio.  
2. Abre el proyecto en Unity (versión compatible con URP + Input System + Cinemachine + DOTween).  
3. Carga la escena `Menu.unity`.  
4. Desde el menú puedes acceder a la demo de combate o al primer capítulo.  
5. Si quieres probar o modificar datos (armas, estadísticas, enemigos), edita los correspondientes **ScriptableObjects** en `Assets/Scripts/ScriptableObjects/`.  

> ⚠️ Asegúrate de tener instalado **DOTween**, **Cinemachine**, y que tu proyecto esté configurado con **Universal Render Pipeline (URP)**.  

---

## Planes futuros / Próximos pasos

- Expandir árbol de habilidades, colección de objetos, inventario y sistema de coleccionables (como ya planeaste).  
- Integrar animaciones UI y 3D para selección de habilidades, menú de inventario, etc.  
- Refinar sistema de combate y comportamiento de enemigos: esquives, parry, revivir, sistema de intentos.  
- Añadir más escenas, narrativa, cinemáticas para capítulos del juego completo.  
- Mejorar sistema de datos y configuración mediante ScriptableObjects para facilitar extensibilidad.  

---

## Licencia

*(Aquí indica la licencia que elijas: MIT, GPL, o la que decidas definir — por ejemplo:)*  

Este proyecto está bajo la licencia **MIT** — ver archivo `LICENSE` para más detalles.  

