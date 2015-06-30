	
	--------------------
	
	Graphics Practical 3
	
	--------------------
	
	Students:
	
		- Gerwin van der Lugt	4261216
		- Adriaan Kisjes		4279093
		- Tomas Billum			4161882
	
	--------------------
	
	Work split:
	
		Adriaan and Tomas did the Geometry math and primitive intersections.
		Gerwin did the BVH tree and ModelLoader.
		
	--------------------
	
	Important:
	
		Our ray tracer uses a BVH tree as optimization technique, by default it will
		load the contents of the BVH tree from a file (main.bvh) to limit loading time.
		If the positions of any of the vertices/polygons on the screen is changed, the
		regenerateBvhTree must be set to true. (In Game1 -> Initialize -> in the Engine
		constructor).
		
	--------------------
		
	Features implemented:
		
		- Full whitted style ray tracking
		- Capability to load FBX models for showing
		- Serializable BVH Tree
		- Glass (refraction/reflection)
		- Reflective surfaces
		
	--------------------
	
	The Demo:
			
		When the game starts it automatically loads the demo scene, the following keys
		can be used to zoom the camera and move around the bunny's.
		
			- Up/Down 		: Zoom in/Zoom out
			- Left/Right 	: Move around the bunny's
			
		In its default configuration the ray tracer renders 14906 polygons at a frame
		rate of about 1.5 FPS.
		
		The three bunny's are each of a different color but from the same model, the
		BVH tree of the scene is pre-generated and stored in
		'GraphicsPractical3\GraphicsPractical3\GraphicsPractical3\bin\x86\Debug' and
		'GraphicsPractical3\GraphicsPractical3\GraphicsPractical3\bin\x86\Release'.
		
		When the pregenerateBvhTree flag is set to true OR 'main.bvh' can't be found
		the BVH tree is generated and stored anyways. (This takes a few minutes!)
	
	
