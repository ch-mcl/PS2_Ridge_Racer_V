import array
import io
import mathutils

import bmesh
import bpy

from .rrv_model import Mesh, Model

#Names
NAME_MATERIAL = 'material_{0:03}'

#Storage Mesh datas
class Mesh_data:
    def __init__(self):
        self.normals = [] #Collect for "Custom Split Normals"
        #self.color0s = []
        self.uvs = []


def generate_mesh(rrv_mesh: Mesh, idx: int):
    mesh_name = 'mesh_{0:04}'.format(idx)
    bl_mesh = bpy.data.meshes.new(mesh_name)
    bl_obj = bpy.data.objects.new(mesh_name, bl_mesh)

    bpy.context.collection.objects.link(bl_obj)
    bpy.context.view_layer.objects.active = bl_obj
    bl_obj.select_set(True)
    bl_mesh = bpy.context.object.data
    bm = bmesh.new()


    mesh_data = Mesh_data() # Store Normals, UVs, VertexColors, etc
    
    for submesh_cnt, submesh in enumerate(rrv_mesh.submeshs):
        for sur_cnt, surface in enumerate(submesh.surfaces):
            vtx_cnt = 0
            is_cw = False
            for vtx in surface.vertexs:
                #Position
                pos = mathutils.Vector((vtx.pos[0], vtx.pos[1], vtx.pos[2]))
                v = bm.verts.new(pos)
                
                #UV
                uv = mathutils.Vector(( vtx.uv[0], vtx.uv[1] ))
                mesh_data.uvs.append(uv)

                #Normal
                normal = mathutils.Vector(( vtx.normal[0], vtx.normal[1], vtx.normal[2] ))
                v.normal = normal
                mesh_data.normals.append(normal)

                #Generate Face
                if vtx_cnt > 1:
                    #print('is_cs: {0}'.format(is_cw))
                    v2 = v
                    if is_cw == True:
                        face = bm.faces.new((v2, v1, v0))
                        is_cw = False
                    else:
                        face = bm.faces.new((v0, v1, v2))
                        is_cw = True
                    face.material_index = idx

                    if vtx_cnt == 2:
                        #compare blender face and rrv model normal
                        _normals = mesh_data.normals[-3:]
                        _face_normal = sum(_normals, mathutils.Vector()) / 3.0
                        _bl_face_normal = mathutils.geometry.normal(v0.co, v1.co, v2.co)
                        dot_res = _bl_face_normal.dot(_face_normal)
                        if (dot_res < 0):
                            #flipping generated face
                            face.normal_flip()
                            #inversing next faces
                            is_cw = False if is_cw else True


                    v0 = v1
                    v1 = v2
                elif vtx_cnt == 1:
                    v1 = v
                elif vtx_cnt == 0:
                    v0 = v
                vtx_cnt = vtx_cnt + 1
            
        #Material
        bl_mat = bpy.data.materials.new(name=NAME_MATERIAL.format(submesh_cnt))
        bl_obj.data.materials.append(bl_mat)
    bm.to_mesh(bl_mesh)
    bm.free()
    
    #uv
    channel_name = "uv0"
    try:
        bl_mesh.uv_layers[channel_name].data
    except:
        #Generate UV
        bl_mesh.uv_layers.new(name = channel_name)
    for i, loop in enumerate(bl_mesh.loops):
        bl_mesh.uv_layers[channel_name].data[i].uv = mesh_data.uvs[loop.vertex_index]
    
    #normal
    bl_mesh.polygons.foreach_set("use_smooth", [True] * len(bl_mesh.polygons))
    bl_mesh.use_auto_smooth = True
    bl_mesh.normals_split_custom_set_from_vertices(mesh_data.normals)
    
    #Object Mode
    bpy.ops.object.mode_set(mode='OBJECT', toggle=False)
    bpy.context.object.rotation_euler[0] = -1.5708


def load(filepath: str):
    # open files
    with open(filepath, 'rb') as file:
        # Parse
        model = Model()
        model.unpack(file)

        # Generate Mesh
        for idx, rrv_mesh in enumerate(model.meshs):
            generate_mesh(rrv_mesh, idx)
        
    file.close()