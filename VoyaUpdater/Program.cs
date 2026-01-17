/* * Developer: Cave Arnold
 * AI Assistant: Gemini
 * Date: January 13, 2026
 * Version: 1.0.0
 * * Abstraction: 
 * This is a Windows Forms application designed to manually update the balance of the 
 * "Voya - Cave's 401(k)" account in the "Guyton-Klinger-Withdrawals" database.
 * * Logic Flow:
 * 1. UI Initialization: Renders a simple dialog with a label, text box, and update button.
 * 2. Input Handling: Accepts raw text input for the balance (supporting commas and '$').
 * 3. Database Interaction: Connects to LocalDB using Microsoft.Data.SqlClient.
 * 4. Execution: Calls the stored procedure 'dbo.usp_AddManualBalance', passing the raw input.
 * - The Stored Procedure handles data cleaning (stripping non-numeric chars) and 
 * conversion to the MONEY data type.
 * 5. Completion: Displays a success message and immediately terminates the application.
 */

using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace VoyaUpdater
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ManualBalanceForm());
        }
    }

    public class ManualBalanceForm : Form
    {
        private Label lblInstruction;
        private TextBox txtBalance;
        private Button btnUpdate;

        // Connection string for LocalDB and your specific database
        // Added TrustServerCertificate=True to prevent SSL errors with LocalDB
        private string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=Guyton-Klinger-Withdrawals;Trusted_Connection=True;TrustServerCertificate=True;";

        public ManualBalanceForm()
        {
            // 1. Setup the Form Window
            this.Text = "Update Voya Balance";
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 2. Create the Label
            lblInstruction = new Label();
            lblInstruction.Text = "Enter current 💰Voya - Cave's 401(k) Balance:";
            lblInstruction.Location = new Point(20, 30);
            lblInstruction.AutoSize = true;
            lblInstruction.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // 3. Create the Text Box
            txtBalance = new TextBox();
            txtBalance.Location = new Point(20, 60);
            txtBalance.Width = 340;
            txtBalance.Font = new Font("Segoe UI", 12F, FontStyle.Regular);

            // 4. Create the Update Button
            btnUpdate = new Button();
            btnUpdate.Text = "Update";
            btnUpdate.Location = new Point(260, 110);
            btnUpdate.Size = new Size(100, 35);
            btnUpdate.Click += new EventHandler(BtnUpdate_Click);

            // 5. Add Controls to Form
            this.Controls.Add(lblInstruction);
            this.Controls.Add(txtBalance);
            this.Controls.Add(btnUpdate);

            // Allow pressing "Enter" to trigger the button
            this.AcceptButton = btnUpdate;
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            string inputBalance = txtBalance.Text.Trim();

            if (string.IsNullOrEmpty(inputBalance))
            {
                MessageBox.Show("Please enter a balance.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("dbo.usp_AddManualBalance", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Pass the raw string input; the Stored Procedure handles cleaning ($ and ,)
                        cmd.Parameters.AddWithValue("@BalanceInput", inputBalance);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Balance updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Close the application immediately after success
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating database:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}