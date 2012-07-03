

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;"
ASSEMBLY = bin/Debug/Banshee.Renamer.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

BANSHEE_RENAMER_DLL_MDB_SOURCE=bin/Debug/Banshee.Renamer.dll.mdb
BANSHEE_RENAMER_DLL_MDB=$(BUILD_DIR)/Banshee.Renamer.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = dmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/Banshee.Renamer.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

BANSHEE_RENAMER_DLL_MDB=

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(BANSHEE_RENAMER_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(BANSHEE_RENAMER_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include Banshee.Renamer.config

FILES = $(BANSHEE_RENAMER_CS) 

DATA_FILES = 

RESOURCES = $(BANSHEE_RENAMER_RESOURCES)

EXTRAS = $(BANSHEE_RENAMER_EXTRAS)

REFERENCES = $(BANSHEE_RENAMER_REFS)

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

BANSHEE_RENAMER_PC = $(BUILD_DIR)/banshee.renamer.pc

$(eval $(call emit-deploy-wrapper,BANSHEE_RENAMER_PC,banshee.renamer.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)

if ENABLE_TESTS
test: $(ASSEMBLY)
	nunit-console $(ASSEMBLY)
endif