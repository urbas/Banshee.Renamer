dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([banshee-renamer], [1.0.2])
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR

AC_PROG_INSTALL

AC_PATH_PROG(MARKDOWN, markdown, no)
if test "x$MARKDOWN" = "xno"; then
        AC_MSG_ERROR([markdown not found])
fi

AC_PATH_PROG(DMCS, dmcs, no)
if test "x$DMCS" = "xno"; then
        AC_MSG_ERROR([dmcs not found])
fi


AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'DEBUG' Configuration [default=YES]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test x$enable_debug = xyes)
if test "x$enable_debug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi

AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'RELEASE' Configuration [default=NO]]),
		enable_release=yes, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test x$enable_release = xyes)

AC_ARG_ENABLE(tests,
	AC_HELP_STRING([--enable-tests],
		[Build NUnit tests into the assembly. [default=NO]]),
		enable_tests=yes, enable_tests=no)
AM_CONDITIONAL(ENABLE_TESTS, test x$enable_tests = xyes)

if test "x$enable_release" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
	AM_CONDITIONAL(ENABLE_DEBUG, true)
	enable_debug=yes
fi


dnl package checks, common for all configs
PKG_CHECK_MODULES([MONO], [mono >= 2.10.5])
PKG_CHECK_MODULES([DOTNET35], [dotnet35 >= 2.10.5])
PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])
PKG_CHECK_MODULES([GTK_DOTNET_20], [gtk-dotnet-2.0])
PKG_CHECK_MODULES([BANSHEE_CORE], [banshee-core >= 2.2.1])
PKG_CHECK_MODULES([BANSHEE_SERVICES], [banshee-services >= 2.2.1])
PKG_CHECK_MODULES([BANSHEE_THICKCLIENT], [banshee-thickclient >= 2.2.1])
PKG_CHECK_MODULES([BANSHEE_HYENA], [banshee-hyena >= 2.2.1])
PKG_CHECK_MODULES([TEMPLATE_SHARP], [template-sharp >= 1.0.2])

dnl package checks, per config

dnl Checks for TESTS:
if test "x$enable_tests" = "xyes" ; then
	PKG_CHECK_MODULES([NUNIT],[mono-nunit >= 2.10.0])
fi


AC_CONFIG_FILES([
banshee-renamer.pc
Makefile
AssemblyInfo.cs
packaging/Fedora/banshee-renamer.spec
])

AC_OUTPUT
