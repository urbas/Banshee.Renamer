ASSEMBLY = Banshee.Renamer
TARGET = library
LINK = $(REF_EXTENSION_RENAMER)
INSTALL_DIR = $(EXTENSIONS_INSTALL_DIR)

SOURCES =  \
	Banshee.Renamer/DbTrackFilenameEngineV1.cs \
	Banshee.Renamer/RenamerService.cs \
	Banshee.Renamer/RenamerWindow.cs \
	Banshee.Renamer/SongFilenameTemplates.cs \
	Banshee.Renamer/TemplateStorage.cs \
	Banshee.Renamer/TrackInfoParameterMap.cs \
	gtk-gui/Banshee.Renamer.RenamerWindow.cs \
	gtk-gui/generated.cs

RESOURCES =  \
	Banshee.Renamer.addin.xml \
	gtk-gui/gui.stetic \
	Resources/RenamerActionsUI.xml

include $(top_srcdir)/build/build.mk
