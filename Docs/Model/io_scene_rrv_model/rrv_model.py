import array
import io
import struct


class Vertex:
    fmt = '<8h'
    # 8h
    # 0-7
    fmt_normal_less = '<5h'
    # 5h
    # 0-4

    def __init__(self):
        self.pos = [0.0, 0.0, 0.0]
        self.normal = [0.0, 0.0, 0.0]
        self.uv = [0.0]
        
    def unpack(self, file, is_normal_less):    
        vtx_fmt = self.fmt_normal_less if is_normal_less else self.fmt
        bytes = file.read(struct.calcsize(vtx_fmt))
        buff = struct.unpack_from(vtx_fmt, bytes, 0)
        
        if is_normal_less:
            fx = 1<<8
            self.pos = [ buff[0]/fx, buff[1]/fx, buff[2]/fx ]
            fx = 1<<12
            #Normal not store
            self.uv = [ buff[3]/fx, buff[4]/fx ]
        else:
            fx = 1<<8
            self.pos = [ buff[0]/fx, buff[1]/fx, buff[2]/fx ]
            fx = 1<<12
            self.normal = [ buff[3]/fx, buff[4]/fx, buff[5]/fx ]
            self.uv = [ buff[6]/fx, buff[7]/fx ]


class SurfaceHeader:
    fmt = '<3I2H'
    # 3I   2H
    # 0-2  3,4
    
    def __init__(self):
        self.unk0x00 = 0x00
        self.unk0x04 = 0x00
        self.unk0x08 = 0x00
        self.unk0x0C = 0x00
        self.submesh_length = 0x00
        
    def unpack(self, file):
        bytes = file.read(struct.calcsize(self.fmt))
        buff = struct.unpack_from(self.fmt, bytes, 0)
        
        self.unk0x00 = buff[0]
        self.unk0x04 = buff[1]
        self.unk0x08 = buff[2]
        self.unk0x0C = buff[3]
        self.submesh_length = buff[4]


class VertexHeader:
    fmt = '<4I2H2I1H2B'
    # 4I   2H   2I   1H  2B
    # 0-3  4,5  6,7  8   9,10
    
    def __init__(self):
        self.unk0x00 = 0x00
        self.unk0x04 = 0x00
        self.unk0x08 = 0x00
        self.unk0x0C = 0x00
        self.vertex_length = 0x00
        self.unk0x12 = 0x00
        self.unk0x14 = 0x00
        self.unk0x18 = 0x00
        self.unk0x1C = 0x00
        self.unk0x1E = 0x00
        self.unk0x1F = 0x00
        
    def unpack(self, file):
        bytes = file.read(struct.calcsize(self.fmt))
        buff = struct.unpack_from(self.fmt, bytes, 0)
        
        self.unk0x00 = buff[0]
        self.unk0x04 = buff[1]
        self.unk0x08 = buff[2]
        self.unk0x0C = buff[3]
        self.vertex_length = buff[4]
        self.unk0x12 = buff[5]
        self.unk0x14 = buff[6]
        self.unk0x18 = buff[7]
        self.unk0x1C = buff[8]
        self.unk0x1E = buff[9]
        self.unk0x1F = buff[10]
                

class Surface:
    def __init__(self):
        self.surface_header = SurfaceHeader()
        self.vertex_header = VertexHeader()
        self.vertexs = []
        
    def unpack(self, file, end_adr):
        #print('Surface Adr {0:#X}'.format(file.tell()))
        self.surface_header.unpack(file)
        if (self.surface_header.unk0x00 == 0x00 and self.surface_header.submesh_length == 0x00):
            #End of Submesh
            return
        
        if (end_adr == 0):
            end_adr = file.tell() + (self.surface_header.submesh_length-1) * 0x10
        self.vertex_header.unpack(file)
        vertex_lenght = self.vertex_header.vertex_length & 0x7FFF;
        if (vertex_lenght == 0):
            #force treat as "5 vertex"
            vertex_lenght = 5
        for i in range(vertex_lenght):
            if (file.tell() >= end_adr):
                break
            is_normal_less = (self.vertex_header.unk0x1F == 0x1) or (self.vertex_header.unk0x1F == 0xF)
            if (self.vertex_header.unk0x1F == 0x0):
                file.seek(end_adr, io.SEEK_SET)
                break
            vertex = Vertex()
            vertex.unpack(file, is_normal_less)
            self.vertexs.append(vertex)
        while ((file.tell() % 0x10) > 0):
            file.seek(0x1, io.SEEK_CUR)


class Submesh:
    def __init__(self):
        self.surfaces = []
        
    def unpack(self, file):
        #print('--- Submesh Adr {0:#X} ----'.format(file.tell()))
        start_adr = file.tell()
        surface = Surface()
        surface.unpack(file, 0)
        self.surfaces.append(surface)
        end_adr = start_adr + surface.surface_header.submesh_length * 0x10
        while (file.tell() < end_adr):
            surface = Surface()
            surface.unpack(file, end_adr)
            self.surfaces.append(surface)


class Mesh:
    def __init__(self):
        self.submeshs = []
        
    def unpack(self, file, mesh_end_adr):
        #print('---| Mesh Adr {0:#X} |----'.format(file.tell()))
        while (file.tell() < mesh_end_adr):
            submesh = Submesh()
            submesh.unpack(file)
            self.submeshs.append(submesh)


class Entry:
    fmt = '<2i'
    
    def __init__(self):
        self.adr_ptr = 0x00
        self.tex_ptr = 0x00
        
    def unpack(self, file):
        bytes = file.read(struct.calcsize(self.fmt))
        buff = struct.unpack_from(self.fmt, bytes, 0)

        self.adr_ptr = buff[0]
        self.tex_ptr = buff[1]


class Header:
    fmt = '<1i'
    
    def __init__(self):
        self.length = 0
        self.entrys = []
    
    def unpack(self, file):
        bytes = file.read(struct.calcsize(self.fmt))
        buff = struct.unpack_from(self.fmt, bytes, 0)

        self.length = buff[0]
        
        for entry in range(self.length):
            entry = Entry()
            entry.unpack(file)
            self.entrys.append(entry)


class Model:
    def __init__(self):
        self.header = Header()
        self.meshs = []

    def unpack(self, file):
        self.header.unpack(file)
        #print('Entry Ended Adr {0:#X}'.format(file.tell()))

        for i, entry in enumerate(self.header.entrys):
            mesh_end_adr = 0x00
            if (i+1 >= self.header.length):
                # Set EOF
                file.seek(0x0, io.SEEK_END)
                mesh_end_adr = file.tell()
            else:
                mesh_end_adr = self.header.entrys[i+1].adr_ptr
            file.seek(self.header.entrys[i].adr_ptr)
            mesh = Mesh()
            mesh.unpack(file, mesh_end_adr)
            self.meshs.append(mesh)
        #print('---| Model Ended Adr {0:#X} |---'.format(file.tell()))
