# Bitácora: lo que me costó entender de Creatio y cómo lo resolví

**1. La sección "Languages" no me aparecía.**
Quise poner la interfaz en español y la sección no estaba donde la doc la ubicaba. El System Designer agrupa las opciones distinto según la versión y el único idioma activo por defecto es el inglés. Este planteo en realidad me resultó familiar por venir de Salesforce: el idioma también se maneja por perfil/usuario y las opciones de personalización están repartidas en un setup parecido, así que entendí rápido la lógica aunque la UI no coincidiera con los pasos indicados. Al final lo dejé en inglés: es cosmético y no toca la integración.

**2. El OAuth no mostraba "Server-to-server".**
La doc habla de crear un tipo **Server-to-server (client credentials)**, pero mi trial mostraba un formulario con el toggle **"Public client"**. Me confundió un rato hasta que recordé el modelo OAuth: un *public client* no puede manejar `client_secret`, y para client credentials necesitás un *confidential client*. Apagué el toggle y listo: Creatio me generó el Client Id y el Client secret.

**3. `El usuario actual no tiene permisos suficientes para usar OData`.**
Este fue el que más tiempo me sacó. El token salía perfecto, pero la primera llamada OData caía siempre con ese mensaje. La doc oficial solo dice "grant sufficient permissions to the technical user" sin aclarar cuáles. Acá me salvó haber usado Salesforce antes: ese error me sonó a los típicos objetos/permisos que quedan mal configurados al crear un "integration user", así que encaré el problema buscando permisos del usuario técnico en vez de romperme la cabeza con el token. Lo confirmé con la guía de un integrador real (Plecto): hay que darle **Access to OData** (`CanUseODataService`) y **View any data**. Sin la segunda no se ven ni las Accounts.

**4. `$expand=Type($select=Name)` devuelve 400.**
La doc muestra expands con select anidado como si nada, pero en esta versión revienta. El `$expand` plano (sin subselect) sí funciona y devuelve el lookup completo, así que leo el `Name` de ahí. Ando con más payload del ideal, pero es lo que la API acepta.

**5. La documentación varía por versión y por producto.**
La misma página existe para 7.x, 8.x y 10.x con pasos distintos. La que me funcionó fue la de **no-code customization → base integrations → OAuth 2.0**, no la de on-site deployment (esa es para el Identity Service auto-hosteado y me llevó por el camino equivocado). Y la URL del token no está en la portada de la doc: la sacás del system setting `OAuth20IdentityServerUrl`.

**6. Donde la doc no daba, leí y pregunté.**
Además de las fuentes oficiales me sostuve de un [video de YouTube](https://www.youtube.com/watch?v=m-Z-UpGb1Vo) que muestra el flujo de integración con Creatio bien de cerca, y de un poco de ayuda de IA para entender ciertos funcionamientos, sobre todo cuando la documentación se quedaba corta. No sustituye la doc, pero acelera mucho cuando tenés dudas del tipo "¿en esta versión esto sigue existiendo?".