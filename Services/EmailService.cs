namespace SmartCarePatientPortal.Services
{
    public class EmailService
    {
        public async Task SendAppointmentConfirmationAsync(string patientEmail, string appointmentDetails)
        {
            // Placeholder for email sending logic
            // In a real application, you would integrate with an email service provider
            await Task.Delay(100); // Simulate email sending
            Console.WriteLine($"Email sent to {patientEmail}: {appointmentDetails}");
        }

        public async Task SendAppointmentReminderAsync(string patientEmail, string appointmentDetails)
        {
            await Task.Delay(100);
            Console.WriteLine($"Reminder sent to {patientEmail}: {appointmentDetails}");
        }

        public async Task SendCancellationNoticeAsync(string email, string appointmentDetails)
        {
            await Task.Delay(100);
            Console.WriteLine($"Cancellation notice sent to {email}: {appointmentDetails}");
        }
    }
}