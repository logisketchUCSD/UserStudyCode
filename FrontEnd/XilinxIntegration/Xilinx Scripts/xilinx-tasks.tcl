#
#  $Header: /devl/xcs/repo/env/Components/TclTasksC/export/Tcl/xilinx-tasks.tcl,v 1.4 2006/07/28 23:07:03 drm Exp $
#
#  The main purpose for this file is to implement the xilinx-pkg
#  as a set of load-on-demand commands instead of loading them
#  all in advance.  We still want the commands to appear to be loaded.
#  We also want the on-line help to behave as if the commands are loaded.
# 

# puts "In xilinx-tasks.tcl"
namespace eval xilinx {
    variable short_help
    variable task_lib

    # define the commands
    set ui_path {c:\circuitsimulatorui\bin\debug\circuitsimulatorui}
    set task_lib(collection) libTclTaskCollection
    set short_help(collection) "manage objects in collections"

    set task_lib(object) libTclTaskObject
    set short_help(object) "Obtain basic information about any Xilinx Tcl object"

    set task_lib(partition) libTclTaskPartition
    set short_help(partition) "Support for design reuse"

    set task_lib(process) libTclTaskProcess
    set short_help(process) "Run and manage processes within a Project"

    set task_lib(project) libTclTaskProject
    set short_help(project) "Manage design files and enable processes to work on them"

    set task_lib(search) libTclTaskSearch
    set short_help(search) "search and return matching objects in a collection"
 
    set task_lib(selection) libTclTaskSelection
    set short_help(selection) "Manages current selection of items in design and device resources"
 
    set task_lib(xfile) libTclTaskXfile
    set short_help(xfile) "Add, remove, and manage source files of a Project"
    
    set task_lib(timing_analysis) libTclTaskTimingAnalysis
    set short_help(timing_analysis) "Generate timing analysis reports."
    


    foreach {c} [array names ::xilinx::task_lib] {
	if { [info commands "::xilinx::$c"] ne "::xilinx::$c" } {
	    # puts $c
	    #
	    # WARNING: Changes to how tasks are loaded must also be
	    # made in *2* places in the "help" proc below.
	    #
	    set cmd "proc $c args \{
            global errorCode 

	    load libCitI_CoreStub[info sharedlibextension]
            load libUtilI_UtilStub[info sharedlibextension]
            load libTcltaskI_TaskStub[info sharedlibextension]

	    Xilinx::CitP::FactoryLoadTclTask \
               $::xilinx::task_lib($c)[info sharedlibextension]
	    Xilinx::CitP::SetupArgs before \$args 
            set ret \[catch \{uplevel 1 ::xilinx::$c \$args \} result\]
	    Xilinx::CitP::SetupArgs after \$args 
            if \{\$ret == 0\} \{ \
            return \$result \
            \} else \{ \
               return  -code \$ret -errorcode \$errorCode  \$result\
            \}
            \}"
	    # puts $cmd
	    eval $cmd
	    namespace export $c
	}
    }

    

    set cmd {proc sketch_draw args {
	catch {project close} msg
	exec $ui_path
	}}

    eval $cmd
    namespace export sketch_draw

    proc help args {
	set hlp "I'm sorry, we couldn't understand the command:\n \"help "
	append hlp [join $args " "]
	append hlp "\"\nPlease type \"help\" or \"help help\" for guidance."
	if { 0 == [llength $args] } {
	    # When there are no arguments, list the commands
	    # with short help and explanation of how to get more.
	    set hlp "The following is a list of the Xilinx-specific Tcl commands\n"
	    append hlp "with a short description of the purpose of each command.\n\n"
	    set max_length 0
	    foreach {c} [lsort [array names ::xilinx::short_help]] {
		if { [string length $c] > $max_length } { 
		    set max_length [string length $c]
		}
	    }
	    foreach {c} [lsort [array names ::xilinx::short_help]] {
		append hlp [format " %${max_length}s -- %s\n" $c $::xilinx::short_help($c)]
	    }
	    append hlp "
For additional help on a specific command, type

  help <command name>

where <command name> is the name of the command you want to know more 
about.  For example:

  help help

generates more information about the help command.\n"
	} elseif { 1 == [llength $args] } {
	    set cmd_name [lindex $args 0]
	    if { $cmd_name eq "help" } {
		set hlp "
The help command has three forms.

First, executing help without any arguments prints
a list of xilinx-specific Tcl commands with a short
description of each command.

Second, executing help with one argument -- the
name of another xilinx-specific Tcl command --
prints a more detailed description of that command.
For many commands, this includes a list of sub-commands.

Third, help can be given two arguments.  In this case,
the first argument is a xilinx-specific Tcl command
name and the second argument is a sub-command of
the command specified.  For example:
  help object name
prints detailed help for the \"name\" subcommand
of the \"object\" command.
"
 	    } elseif { [info exists ::xilinx::task_lib($cmd_name)] } {
		load "libCitI_CoreStub[info sharedlibextension]"
		load "libUtilI_UtilStub[info sharedlibextension]"
		load "libTcltaskI_TaskStub[info sharedlibextension]"
		Xilinx::CitP::FactoryLoadTclTask "$::xilinx::task_lib($cmd_name)[info sharedlibextension]"
		set hlp [::xilinx::${cmd_name} -help]
	    }
	} elseif { 2 == [llength $args] } {
	    set cmd_name [lindex $args 0]
	    if { [info exists ::xilinx::task_lib($cmd_name)] } {
		load "libCitI_CoreStub[info sharedlibextension]"
		load "libUtilI_UtilStub[info sharedlibextension]" 
		load "libTcltaskI_TaskStub[info sharedlibextension]"
		Xilinx::CitP::FactoryLoadTclTask "$::xilinx::task_lib($cmd_name)[info sharedlibextension]"
		set hlp [::xilinx::${cmd_name} [lindex $args 1] -help]
	    }
	}
	set hlp
    }
    namespace export help

}

