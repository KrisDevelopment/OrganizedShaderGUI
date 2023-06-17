# Organized Shader GUI
GUI with automated shader properties grouping. 

### Features:
- Automatically organize ungrouped material properties by their underlying type (Eg. Textures, Ranges, Floats, etc.)
- Group properties together that follow the convetion [Group GroupName].
- Search for properties
- Display enabled passes.
- "Edit Shader" button that also works with Amplify Shader Editor.

Really useful for Node-based or AI-generated shaders that have a lot of properties exposed.

![image](https://github.com/KrisDevelopment/OrganizedShaderGUI/assets/35272196/65d361b8-855a-411b-9e15-5ce0d036d649)

### Adding it to your shaders

The name of the Shader GUI is "KrisDevelopment.OrganizedShaderGUI". Placing this string in the Custom Shader GUI field of Amplify or as CustomEditor in your HLSL code will draw the shaders with it.

HLSL Example:

![image](https://github.com/KrisDevelopment/OrganizedShaderGUI/assets/35272196/1a9a29c6-acd6-4705-8314-c9367d2f882f)

Amplify Example:

![image](https://github.com/KrisDevelopment/OrganizedShaderGUI/assets/35272196/f6f9d802-2bf4-4f0c-9ea8-c840c2e53771)

Shader Graph Example:

![image](https://github.com/KrisDevelopment/OrganizedShaderGUI/assets/35272196/8fe45aa1-54e5-4c77-aa8f-3a732a3ad750)


### Custom grouping

Groups are defined directly in the property's public name. For example, a property named ` [Group World-Blend-Options] "World blend enabled" ` will appear as part of a new World-Blend-Options group. Supports only one word for the group name, deeming hyphens necessary if the words are compound.

HLSL example with adding properties to a Base-Properties group:

![image](https://github.com/KrisDevelopment/OrganizedShaderGUI/assets/35272196/613fa4b3-a672-46e1-bb92-9709bcb22a4b)

The same approach can be applied for all types of shader tech, that expose the display name.

### Search
The GUI also supports searching for properties. Here's an example:

![image](https://github.com/KrisDevelopment/OrganizedShaderGUI/assets/35272196/e118ad2d-6d25-4b32-9119-50c8b3241a2f)
