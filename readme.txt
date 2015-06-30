    
    --------------------------------------------
    
    Graphics Practical 3    /    H6 : Ray Tracer
    
    --------------------------------------------
    
    Students:
    
        - Gerwin van der Lugt   4261216
        - Adriaan Kisjes        4279093
        - Tomas Billum          4161882
    
    --------------------------------------------
    
    Work split:
    
        Adriaan and Tomas did the Geometry math and primitive intersections.
        Gerwin did the BVH tree and MyModel FBX model loader.
        
    --------------------------------------------
    
    Important:
    
        Our ray tracer uses a BVH tree as optimization technique, by default it will
        load the contents of the BVH tree from a file (main.bvh) to limit loading time.
        If the positions of any of the vertices/polygons on the screen is changed, the
        regenerateBvhTree must be set to true. (In Game1 -> Initialize -> in the Engine
        constructor).
        
    --------------------------------------------
        
    Features implemented:
        
        - Whitted style ray tracing
        - Capability to load FBX models
        - Serializable BVH Tree
        - Glass (refraction/reflection)
        - Reflective surfaces
        
    --------------------------------------------
    
    Code layout:
    
        RayTracing.cs :
            -   Consists of the Eye and Screen class and the Engine class.
            -   The Engine class has all function related to illumination,
                tracing and loads the BVHTree.
            -   It also controls whether the BVHTree is loaded from a file or generated.
            
        BVHTree.cs :
            -   Implements the BVH tree: the Engine class calls TryHit on its BVHTree
                to intersect it with a ray.
        
        LineReader.cs :
            -   Used for deserialization of BVH tree.
        
        Material.cs :
            -   Every Primitive has an instance of this class which among other things controls
                the color.
        
        Geometry.cs   :
            -   Has the abstract Primitive class and it subclasses Sphere/Triangle.
            -   Any primitive implements the following functions: Normal(), HitDistance() -> float,
                Hit(), Center(), BoundingBox(). All primitives also contain Material info.
            -   Contains the definition for BoundingBox.
            -   Contains Model, both Primitives and MyModel use this as parent class.
            
        ModelLoader.cs :
            -   Loads Model classes (Primitives and MyModel) and extracts all primitives.
            -   Used by Engine to get a list of all primitives in the scene.
        
        MyModel.cs :
            -   Loads a set of primitives from a FBX-file. Can be loaded by the ModelLoader.
            
        Game1.cs :
            -   Loads all needed primitives (from Models) with the ModelLoader and starts
                the ray tracing Engine class.
            -   Calls Update() on the engine.
            -   Updates FrameRate in Window Title.
            -   Detects key strokes and changes angle and zoom accordingly.
            -   Calculates Eye position and direction after moving.
        
    --------------------------------------------
    
    The Demo:
            
        When the game starts it automatically loads the demo scene, the following keys
        can be used to zoom the camera and move around the bunny's.
        
            - Up/Down       : Zoom in/Zoom out
            - Left/Right    : Move around the bunny's
            
        In its default configuration the ray tracer renders 14906 polygons at a frame
        rate of about 1.5 FPS.
        
        The three bunny's are each of a different color but from the same model, the
        BVH tree of the scene is pre-generated and stored in
        'GraphicsPractical3\GraphicsPractical3\GraphicsPractical3\bin\x86\Debug' and
        'GraphicsPractical3\GraphicsPractical3\GraphicsPractical3\bin\x86\Release'.
        
        When the pregenerateBvhTree flag is set to true OR 'main.bvh' can't be found
        the BVH tree is generated and stored anyways. (This takes a few minutes!)
    
    
