using System;
using System.Collections;

namespace CommandManagement
{
	/// <summary>
	/// Summary description for CommandManager.
	/// </summary>
	public class CommandManager
	{
		private Stack undoCommands;
		private Stack redoCommands;

		
		/// <summary>
		/// Constructor
		/// 
		/// Utilizes an Undo and Redo stack to handle Commands
		/// </summary>
		public CommandManager()
		{
			undoCommands = new Stack();
			redoCommands = new Stack();
		}
        

		/// <summary>
		/// Executes a Command and places it onto the Undo stack, if appropriate.
		/// </summary>
		/// <param name="command">Command to execute</param>
		public void ExecuteCommand(Command command)
		{
			command.Execute();
			
			if (command.IsUndoable())
				undoCommands.Push(command);
		}


		/// <summary>
		/// Undos the last (undoable) Command.
		/// </summary>
		/// <returns>True if a Command is unexecuted</returns>
		public bool Undo()
		{
			if (undoCommands.Count > 0)
			{
				Command currCommand = (Command)undoCommands.Pop();
				currCommand.UnExecute();

				redoCommands.Push(currCommand);

				return true;
			}
			else
			{
				return false;
			}
		}
		

		/// <summary>
		/// Redos the last undone Command.
		/// </summary>
		/// <returns>True if a Command is re-executed</returns>
		public bool Redo()
		{
			if (redoCommands.Count > 0)
			{
				Command currCommand = (Command)redoCommands.Pop();
				currCommand.Execute();

				undoCommands.Push(currCommand);

				return true;
			}
			else
			{
				return false;
			}
		}
	

		/// <summary>
		/// Clears both the Undo and Redo stacks.
		/// </summary>
		public void ClearStacks()
		{
			undoCommands.Clear();
			redoCommands.Clear();
		}
	}
}
