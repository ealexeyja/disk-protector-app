using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;

namespace DiskProtectorApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            LoadDrives();
        }

        private void LoadDrives()
        {
            listViewDrives.Items.Clear();
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in allDrives)
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    ListViewItem item = new ListViewItem(drive.Name);
                    item.SubItems.Add(drive.VolumeLabel);
                    item.SubItems.Add(drive.DriveType.ToString());
                    item.SubItems.Add(FormatBytes(drive.TotalSize));
                    item.SubItems.Add(IsDriveProtected(drive.Name) ? "✅ Protegido" : "❌ No protegido");
                    
                    item.Tag = drive;
                    listViewDrives.Items.Add(item);
                }
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n2} {suffixes[counter]}";
        }

        private bool IsDriveProtected(string drivePath)
        {
            try
            {
                var di = new DirectoryInfo(drivePath);
                var ds = di.GetAccessControl();
                
                var usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                
                foreach (FileSystemAccessRule rule in ds.GetAccessRules(true, true, typeof(SecurityIdentifier)))
                {
                    if (rule.IdentityReference == usersSid && 
                        rule.AccessControlType == AccessControlType.Deny &&
                        (rule.FileSystemRights & FileSystemRights.WriteData) == FileSystemRights.WriteData)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking protection: {ex.Message}");
                return false;
            }
        }

        private void btnProtect_Click(object sender, EventArgs e)
        {
            if (listViewDrives.SelectedItems.Count == 0)
            {
                MessageBox.Show("Por favor, selecciona al menos una unidad.", "Advertencia", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (ListViewItem item in listViewDrives.SelectedItems)
            {
                if (item.Tag is DriveInfo drive)
                {
                    try
                    {
                        ProtectDrive(drive.Name);
                        item.SubItems[4].Text = "✅ Protegido";
                        MessageBox.Show($"Unidad {drive.Name} protegida correctamente.", "Éxito", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show($"Se requieren permisos de administrador para proteger {drive.Name}.", "Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al proteger {drive.Name}: {ex.Message}", "Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ProtectDrive(string drivePath)
        {
            var di = new DirectoryInfo(drivePath);
            var ds = di.GetAccessControl();
            
            var usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            
            var denyRule = new FileSystemAccessRule(
                usersSid,
                FileSystemRights.WriteData | FileSystemRights.Delete | 
                FileSystemRights.DeleteSubdirectoriesAndFiles | FileSystemRights.ChangePermissions,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Deny);
            
            ds.AddAccessRule(denyRule);
            di.SetAccessControl(ds);
        }

        private void btnUnprotect_Click(object sender, EventArgs e)
        {
            if (listViewDrives.SelectedItems.Count == 0)
            {
                MessageBox.Show("Por favor, selecciona al menos una unidad.", "Advertencia", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (ListViewItem item in listViewDrives.SelectedItems)
            {
                if (item.Tag is DriveInfo drive)
                {
                    try
                    {
                        UnprotectDrive(drive.Name);
                        item.SubItems[4].Text = "❌ No protegido";
                        MessageBox.Show($"Protección eliminada de {drive.Name}.", "Éxito", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show($"Se requieren permisos de administrador para desproteger {drive.Name}.", "Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al desproteger {drive.Name}: {ex.Message}", "Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void UnprotectDrive(string drivePath)
        {
            var di = new DirectoryInfo(drivePath);
            var ds = di.GetAccessControl();
            var usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            
            var rules = ds.GetAccessRules(true, true, typeof(SecurityIdentifier));
            
            foreach (AuthorizationRule rule in rules)
            {
                if (rule is FileSystemAccessRule fsRule && 
                    fsRule.IdentityReference == usersSid &&
                    fsRule.AccessControlType == AccessControlType.Deny)
                {
                    ds.RemoveAccessRule(fsRule);
                }
            }
            
            di.SetAccessControl(ds);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadDrives();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "🔒 PROTECTOR DE DISCOS INTERNOS\n\n" +
                "1. Selecciona las unidades internas que deseas proteger\n" +
                "2. Haz clic en 'Proteger' para evitar escrituras accidentales\n" +
                "3. Los usuarios no administradores podrán leer pero no modificar\n" +
                "4. Usa 'Desproteger' para restaurar permisos completos\n\n" +
                "⚠️ NOTA: La aplicación debe ejecutarse como Administrador",
                "Ayuda", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information
            );
        }
    }
}
