import bpy
import importlib

from . import import_rrv_model

bl_info= {
    "name": "Ridge Racer V 3D Model",
    "author": "chmcl95",
    "version": (0, 1, 0),
    "blender": (2, 80, 0),
    "location": "File > Import > Ridge Racer V",
    "description": "Trying to imports a Ridge Racer V 3D model",
    "category": "Import",
}


if "bpy" in locals():
    import importlib
    if "import_rrv_model" in locals():
        importlib.reload(import_rrv_model)

from bpy.props import (
        StringProperty,
        )


#Import Ridge Racer V Model
class IMPORT_SCENE_MT_RRVMODLE(bpy.types.Operator):
    bl_idname = "import_scene.rrv_model"
    bl_label = "Import RRV Model"
    bl_description = "Trying to imports a Ridge Racer V 3D model"
    bl_options = {'REGISTER', 'UNDO'}

    filepath: StringProperty(
        name="File Path",
        description="Filepath used for importing the Ridge Racer V 3D Modle file",
        maxlen=1024)

    def execute(self, context):
        keywords = self.as_keywords()
        import_rrv_model.load(self.filepath)
        return {'FINISHED'}

    def invoke(self, context, event):
        wm = context.window_manager
        wm.fileselect_add(self)
        return {'RUNNING_MODAL'}


def menu_func_import(self, context):
    self.layout.operator(IMPORT_SCENE_MT_RRVMODLE.bl_idname, text="Ridge Racer V")


classes = (
    IMPORT_SCENE_MT_RRVMODLE,
)


def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    
    bpy.types.TOPBAR_MT_file_import.append(menu_func_import)


def unregister():
    bpy.types.TOPBAR_MT_file_import.remove(menu_func_import)

    for cls in classes:
        bpy.utils.unregister_class(cls)


if __name__ == "__main__":
    register()
