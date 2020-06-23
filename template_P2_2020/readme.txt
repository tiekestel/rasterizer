Students:
Sietze Riemersma - 6855369
Tieke Stellingwerf - 6269192

Implemented features
- Basic rasterizer
- Multiple lights which can be modified at runtime (streetlights (spotlight), car headlights (spotlight), campfire (pointlight), sun (directional light))
- Spotlights (5 streetlights, 2 car headlights)
- Cube mapping (carrot and skydome)
- Normal mapping (grass plane and train)
- Shadows (for directional light and pointlight (sun and campfire))
- HDR glow (campfire)

Hierarchy
- Grass floor
	- Traintracks
	- Train
	- Roads
	- McDonalds
	- House
	- Igloos
	- Campfire
		- Campfire light (pointlight)
	- Trees
	- Streetlamps
		- Streetlamp light (spotlight)
	- Car
		- Wheels
		- Headlights (spotlight)
- Sun (directional light)

Work division
- Basic rasterizer: 		Sietze Riemersma and Tieke Stellingwerf
- Directional light: 		Sietze Riemersma and Tieke Stellingwerf
- Pointlight: 			Sietze Riemersma and Tieke Stellingwerf
- Spotlight: 			Sietze Riemersma and Tieke Stellingwerf
- Cube mapping: 		Sietze Riemersma
- Normal mapping: 		Sietze Riemersma
- Shadows: 			Sietze Riemersma
- Scene: 			Sietze Riemersma and Tieke Stellingwerf
- HDR glow: 			Sietze Riemersma
- Normal post processing	Sietze Riemersma and Tieke Stellingwerf

Instructions
- Key up: move the car (and the camera) to the front
- Key down: move the car (and the camera) to the back
- Key left: move the car (and the camera) to the left
- Key right: move the car (and the camera) to the right
- Scroll: zoom in and out
- Key H: put headlights of the car on and off

Sources
https://learnopengl.com