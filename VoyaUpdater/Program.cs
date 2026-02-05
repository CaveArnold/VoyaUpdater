/* * Developer: Cave Arnold
 * AI Assistant: Gemini
 * Date: February 5, 2026
 * Version: 1.2.1
 * * Abstraction: 
 * This is a Windows Forms application designed to manually update the balance of the 
 * "Voya - Cave's 401(k)" account in the "Guyton-Klinger-Withdrawals" database.
 * * Logic Flow:
 * 1. UI Initialization: Renders a form with Dark Mode styling (Dark background, Bright Green text).
 * - All text elements are center-aligned.
 * 2. Data Retrieval: On load, connects to the database and calls 'dbo.usp_GetVoyaBalance' 
 * to fetch and display the most recent account value.
 * 3. Input Handling: Accepts raw text input for the new balance (supporting commas and '$').
 * 4. Database Interaction: Connects to LocalDB using Microsoft.Data.SqlClient.
 * 5. Execution: Calls the stored procedure 'dbo.usp_AddManualBalance', passing the raw input.
 * - The Stored Procedure handles data cleaning (stripping non-numeric chars) and 
 * conversion to the MONEY data type.
 * - STRICT VALIDATION: The Stored Procedure enforces a "One Entry Per Day" rule. If a record 
 * already exists for the current date, an exception is raised and displayed to the user.
 * 6. Completion: Displays a success message and immediately terminates the application.
 * * Version History:
 * - v1.0.0 (Jan 13, 2026): Initial Release. Basic update functionality.
 * - v1.1.0 (Feb 05, 2026): Added current balance retrieval on startup.
 * - v1.2.0 (Feb 05, 2026): UI overhaul (Dark Mode, Lime Green text, Centered Alignment).
 * - v1.2.1 (Feb 05, 2026): Updated database logic to enforce strict "One Entry Per Day" validation.
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
        private Label lblCurrentBalance;
        private Label lblInstruction;
        private TextBox txtBalance;
        private Button btnUpdate;

        // Connection string for LocalDB
        private string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=Guyton-Klinger-Withdrawals;Trusted_Connection=True;TrustServerCertificate=True;";

        // Dark Mode & Style Definitions
        private Color formBackground = Color.FromArgb(30, 30, 30);      // Dark Charcoal
        private Color controlBackground = Color.FromArgb(50, 50, 50);   // Slightly lighter for inputs
        private Color primaryColor = Color.Lime;                        // Bright Terminal Green for contrast

        private Font primaryFont = new Font("Segoe UI", 14F, FontStyle.Bold);
        private Font inputFont = new Font("Segoe UI", 16F, FontStyle.Bold);

        public ManualBalanceForm()
        {
            // 1. Setup the Form Window
            this.Text = "Update Voya Balance";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = formBackground; // Apply Dark Mode Background

            // 2. Create the Current Balance Label
            lblCurrentBalance = new Label();
            lblCurrentBalance.Text = "Current Balance: Loading...";
            lblCurrentBalance.Location = new Point(0, 30); // Start at left edge
            lblCurrentBalance.Width = 480; // Span the width of the form (minus borders)
            lblCurrentBalance.AutoSize = false; // Disable autosize to allow centering
            lblCurrentBalance.TextAlign = ContentAlignment.MiddleCenter; // Center Text
            lblCurrentBalance.Font = primaryFont;
            lblCurrentBalance.ForeColor = primaryColor;

            // 3. Create the Instruction Label
            lblInstruction = new Label();
            lblInstruction.Text = "Enter NEW Balance:";
            lblInstruction.Location = new Point(0, 80);
            lblInstruction.Width = 480;
            lblInstruction.AutoSize = false;
            lblInstruction.TextAlign = ContentAlignment.MiddleCenter; // Center Text
            lblInstruction.Font = primaryFont;
            lblInstruction.ForeColor = primaryColor;

            // 4. Create the Text Box
            txtBalance = new TextBox();
            txtBalance.Location = new Point(50, 120); // Indented to center visually
            txtBalance.Width = 380;
            txtBalance.Font = inputFont;
            txtBalance.ForeColor = primaryColor;
            txtBalance.BackColor = controlBackground; // Dark input background
            txtBalance.TextAlign = HorizontalAlignment.Center; // Center Text inside box
            txtBalance.BorderStyle = BorderStyle.FixedSingle;

            // 5. Create the Update Button
            btnUpdate = new Button();
            btnUpdate.Text = "UPDATE";
            btnUpdate.Location = new Point(50, 190);
            btnUpdate.Size = new Size(380, 50);
            btnUpdate.Font = primaryFont;
            btnUpdate.ForeColor = formBackground; // Dark text on the button
            btnUpdate.BackColor = primaryColor;   // Green button background
            btnUpdate.FlatStyle = FlatStyle.Flat;
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += new EventHandler(BtnUpdate_Click);

            // 6. Add Controls to Form
            this.Controls.Add(lblCurrentBalance);
            this.Controls.Add(lblInstruction);
            this.Controls.Add(txtBalance);
            this.Controls.Add(btnUpdate);

            // Allow pressing "Enter" to trigger the button
            this.AcceptButton = btnUpdate;

            // 7. Fetch the current balance immediately
            GetCurrentBalance();
        }

        private void GetCurrentBalance()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("dbo.usp_GetVoyaBalance", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            decimal currentBal = Convert.ToDecimal(result);
                            lblCurrentBalance.Text = $"Current Balance: {currentBal:C2}";
                        }
                        else
                        {
                            lblCurrentBalance.Text = "Current Balance: $0.00";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblCurrentBalance.Text = "Current Balance: Error";
                // Only show popup for critical connection failures if desired, or just log it
            }
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
                        cmd.Parameters.AddWithValue("@BalanceInput", inputBalance);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Balance updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating database:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}