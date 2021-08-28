# Physics-based-vehicles
[![Physics Based Vehicle](https://img.youtube.com/vi/0A-C57Njkp0/0.jpg)](https://youtu.be/0A-C57Njkp0)

<h2> Common probloblems and solutions </h2>
<b> Roll is slow when SAS is enabled </b>
<p> Remove the rudder. Sometimes it offers too much stability. </p>
<h3></h3>
<b> Vehicle is shaking or snapping </b>
<p> This is most likely a problem with the inertia tensor. Disable all colliders and add a simple box collider to the main vehicle object.
Then scale that collider to roughly the size of your vehicle. </p>
