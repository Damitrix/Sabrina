namespace Sabrina.SankakuModel
{
	public partial class Image
	{
		[System.ComponentModel.DataAnnotations.Schema.NotMapped]
		public bool IsAvailableBool
		{
			get => IsAvailable == 1;
			set => IsAvailable = value == true ? (short)1 : (short)0;
		}
	}
}