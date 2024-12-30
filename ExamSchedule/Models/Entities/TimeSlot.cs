namespace SchedualTest.Models.Entities
{
    public class TimeSlot
    {
        public int SlotId { get; set; }
        public DateTime StartTime { get; set; }
        public double Duration { get; set; }
        //when generate population remove time slot from list
        //when used equal to rooms count
        public int Usage { get; set; }
    }
}
