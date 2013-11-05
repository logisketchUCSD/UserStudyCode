using System;

namespace CommandManagement
{
	/// <summary>
	/// Summary description for Command.
	/// </summary>
	public abstract class Command
	{
		private CommandManager CM;

		/// <summary>
		/// Execute the Command
		/// </summary>
		public abstract void Execute();

		/// <summary>
		/// Unexecute the Command
		/// </summary>
		public abstract void UnExecute();

		/// <summary>
		/// Is the Command undoable?
		/// </summary>
		/// <returns>True iff the Command is undoable</returns>
		public abstract bool IsUndoable();

		/// <summary>
		/// Get the CommandManager
		/// </summary>
		internal CommandManager Manager
		{
			get
			{
				return CM;
			}
			set
			{
				CM = value;
			}
		}
	}
}
