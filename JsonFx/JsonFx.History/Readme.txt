JsonFx.NET - Ajax History Manager

The JsonFx.NET Ajax History Manager allows application state to be maintained and manipulated with
the web browser's back/foward buttons.

- User can "navigate" back/foth through the application history using the web browser's back/forward buttons

- State data may be any JSON-serializable object graph.

- Encapsulated solution does not muddy up the address bar URL

- No additional trips to the server are made to maintain state.

- Initial state may be set via server, additional state calls JsonFx.History.save(...);

- When user changes the state, the application is notified via a callback method.

- In Firefox, state is maintained even after leaving the Ajax page.
