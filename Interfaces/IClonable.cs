namespace AppInterfaces
{
	/// <summary>
	/// Interface for objects, who can be cloned without Utils.DeepClone().
	/// Utils.DeepClone uses serialization\deserialization with BinaryFormatter and it is too slow.
	/// </summary>
	public interface IClonable
	{
		/// <summary>
		/// Return deep copy of the object.
		/// </summary>
		IClonable Clone();
	}
}
