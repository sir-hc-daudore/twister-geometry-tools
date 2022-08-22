# Twister Geometry tools
Unity tools for modifying and placing meshes in a non-destructive way. 

Supports polyline and Bezier curves for morphing and placing meshes.

## Adding the tools into the project
1. Download repository or check out locally.
2. Copy the `Twister` folder into the root of `Assets`.

## Usage

### Mesh Morphing
1. Select a game object with a `Mesh Filter` component, then add new component under the section `Geometry Tools/Morph`. Currently, the options are `Bezier Morph` and `Polyline Morph` components.
2. After adding the new component, set it up as follows:
    1. In the morph component, select the `Setup Mode` option.
    2. In the `Original Mesh` property of the component, select the mesh that mathches the `Mesh Filter` in the same object. A yellow wireframe should show up in the Scene view.
    3. P0 and P1 are the starting and end points respectively for the morphing of the mesh. Using the handles or the input fields in the inspector, place them where the morph tool should use as origins for morphing with the path.
3. Start the mesh morphing by adding some morph anchors under the `Modifer Mode` option.
    1. Add anchors by pressing the + sign in the dropdown list of the component.
    2. Use the handles or the input fields of each anchor element for updating the morphing path. A white line should be drawn along each of the anchors in the Scene view.
4. Press `Update Mesh` button to apply the mesh morphing. A new mesh will be created and assigned to the Mesh Filter.

To restore the object to use the original mesh, press the `Reset Mesh` button.

### Path placing 
TBD
