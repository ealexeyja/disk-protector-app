using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;

namespace DiskProtectorApp
{
    public partial class MainForm : Form
    {
        private string systemDrive = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            systemDrive = GetSystemDrive();
            LoadDrives();
        }

        private string GetSystemDrive()
        {
            var windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            if (string.IsNullOrEmpty(windowsPath))
            {
                return string.Empty;
            }
            
            var rootPath = Path.GetPathRoot(windowsPath);
            return rootPath ?? string.Empty;
        }

        private bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("Esta aplicación debe ejecutarse como Administrador para funcionar correctamente.", 
                                "Permisos Insuficientes", 
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Warning);
            }
        }

        private void LoadDrives()
        {
            listViewDrives.Items.Clear();
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in allDrives)
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    bool isSystemDrive = drive.Name.Equals(systemDrive, StringComparison.OrdinalIgnoreCase);
                    
                    ListViewItem item = new ListViewItem(drive.Name);
                    item.SubItems.Add(drive.VolumeLabel);
                    item.SubItems.Add(drive.DriveType.ToString());
                    item.SubItems.Add(FormatBytes(drive.TotalSize));
                    
                    if (isSystemDrive)
                    {
                        item.SubItems.Add("⚠️ Unidad del Sistema");
                        item.ForeColor = Color.Gray;
                        item.BackColor = Color.LightYellow;
                        item.ToolTipText = "Esta es la unidad del sistema Windows y no puede ser protegida";
                    }
                    else
                    {
                        item.SubItems.Add(IsDriveProtected(drive.Name) ? "✅ Protegido" : "❌ No protegido");
                    }
                    
                    item.Tag = new DriveInfoEx(drive, isSystemDrive);
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
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("Se requieren permisos de administrador para proteger unidades.",
                                "Permisos Insuficientes",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (listViewDrives.SelectedItems.Count == 0)
            {
                MessageBox.Show("Por favor, selecciona al menos una unidad.", "Advertencia", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (ListViewItem item in listViewDrives.SelectedItems)
            {
                if (item.Tag is DriveInfoEx driveEx)
                {
                    if (driveEx.IsSystemDrive)
                    {
                        MessageBox.Show($"No se puede proteger la unidad del sistema ({driveEx.Drive.Name}).", 
                                        "Operación no permitida", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    try
                    {
                        ProtectDrive(driveEx.Drive.Name);
                        item.SubItems[4].Text = "✅ Protegido";
                        MessageBox.Show($"Unidad {driveEx.Drive.Name} protegida correctamente.", "Éxito", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show($"Se requieren permisos de administrador para proteger {driveEx.Drive.Name}.", "Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al proteger {driveEx.Drive.Name}: {ex.Message}", "Error", 
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
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("Se requieren permisos de administrador para desproteger unidades.",
                                "Permisos Insuficientes",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (listViewDrives.SelectedItems.Count == 0)
            {
                MessageBox.Show("Por favor, selecciona al menos una unidad.", "Advertencia", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (ListViewItem item in listViewDrives.SelectedItems)
            {
                if (item.Tag is DriveInfoEx driveEx)
                {
                    if (driveEx.IsSystemDrive)
                    {
                        MessageBox.Show($"No se puede desproteger la unidad del sistema ({driveEx.Drive.Name}).", 
                                        "Operación no permitida", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    try
                    {
                        UnprotectDrive(driveEx.Drive.Name);
                        item.SubItems[4].Text = "❌ No protegido";
                        MessageBox.Show($"Protección eliminada de {driveEx.Drive.Name}.", "Éxito", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show($"Se requieren permisos de administrador para desproteger {driveEx.Drive.Name}.", "Error", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al desproteger {driveEx.Drive.Name}: {ex.Message}", "Error", 
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
            
            bool ruleRemoved = false;
            var rules = ds.GetAccessRules(true, true, typeof(SecurityIdentifier));
            
            foreach (AuthorizationRule rule in rules)
            {
                if (rule is FileSystemAccessRule fsRule && 
                    fsRule.IdentityReference == usersSid &&
                    fsRule.AccessControlType == AccessControlType.Deny)
                {
                    ds.RemoveAccessRule(fsRule);
                    ruleRemoved = true;
                }
            }
            
            if (!ruleRemoved)
            {
                rules = ds.GetAccessRules(true, true, typeof(NTAccount));
                foreach (AuthorizationRule rule in rules)
                {
                    if (rule is FileSystemAccessRule fsRule && 
                        fsRule.IdentityReference.Value.Contains("Users") &&
                        fsRule.AccessControlType == AccessControlType.Deny)
                    {
                        ds.RemoveAccessRule(fsRule);
                        ruleRemoved = true;
                    }
                }
            }
            
            di.SetAccessControl(ds);
            
            if (!ruleRemoved)
            {
                ResetInheritance(drivePath);
            }
        }

        private void ResetInheritance(string drivePath)
        {
            try
            {
                var di = new DirectoryInfo(drivePath);
                var ds = di.GetAccessControl();
                
                ds.SetAccessRuleProtection(false, true);
                di.SetAccessControl(ds);
                
                ds = di.GetAccessControl();
                di.SetAccessControl(ds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reseteando herencia: {ex.Message}");
            }
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
                "4. Usa 'Desproteger' para restaurar permisos completos\n" +
                "5. La unidad del sistema (Windows) no puede ser protegida\n\n" +
                "⚠️ NOTA: La aplicación debe ejecutarse como Administrador",
                "Ayuda", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information
            );
        }

        private void listViewDrives_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e?.Item != null && e.Item.Tag is DriveInfoEx driveEx && driveEx.IsSystemDrive)
            {
                MessageBox.Show("Esta es la unidad del sistema Windows. No puede ser protegida o desprotegida por seguridad.", 
                                "Unidad del Sistema", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Item.Selected = false;
            }
        }
    }
}