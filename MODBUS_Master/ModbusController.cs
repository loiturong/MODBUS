using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System;

namespace MODBUS_Master;

public partial class ModbusController : Form
{
    private readonly SerialPort _serialPort;
    private System.Windows.Forms.Timer _modbusReadTimer;
    
    public ModbusController()
    {
        // Initialize serial port
        _serialPort = new SerialPort();
        _serialPort.ReadTimeout = 500;
        
        // Initialize timer
        _modbusReadTimer = new System.Windows.Forms.Timer();
        _modbusReadTimer.Interval = 500;
        _modbusReadTimer.Tick += ModbusReadTimer_Tick;

        InitializeComponent();
        InitializeModbusFunctions();
        
        // disable functioning
        Master_grid.Enabled = false;
        Slave_grid.Enabled = false;
        Function_gb.Enabled = false;
        COMPort_Close.Enabled = false;
    }
    
    private void GetComPorts()
    {
        // Clear existing items
        COMPort_Comselect.Items.Clear();
    
        // Get all available COM ports
        string[] ports = SerialPort.GetPortNames();
    
        // Add ports to combo box
        COMPort_Comselect.Items.AddRange(ports);
    
        // Select first port if available
        if (COMPort_Comselect.Items.Count > 0)
            COMPort_Comselect.SelectedIndex = 0;
    }
    
    private void COMPort_Comselect_DropDown(object sender, EventArgs e)
    {
        GetComPorts();
    }
    
    // Modbus function selection handling
    private void FunctionSelectcb_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selectedFunction = FunctionSelectcb.SelectedItem.ToString();
    
        // Clear existing fields
        Master_grid.Rows.Clear();
    
        switch (selectedFunction)
        {
            case "Read Holding Registers (03)":
                AddField("Slave Address", "", Master_grid);
                AddField("Function", "03", Master_grid);
                AddField("Start Address Hi", "0", Master_grid);
                AddField("Start Address Lo", "0", Master_grid);
                AddField("Number of Points Hi", "0", Master_grid);
                AddField("Number of Points Lo", "0", Master_grid);
                AddField("Error Check", "Yes", Master_grid);
                break;
            // Add other functions latter
            
            default:
                break;
        }
    }
    private void ModbusReadTimer_Tick(object? sender, EventArgs e)
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                int bytesToRead = 5 + (Convert.ToByte(Master_grid.Rows[5].Cells["value"].Value) * 2); // Response format: Slave Address(1) + Function Code(1) + Byte Count(1) + Data(2 * numberOfPoints) + CRC(2)
                byte[] response = new byte[bytesToRead];

                if (_serialPort.BytesToRead >= bytesToRead)
                {
                    _serialPort.Read(response, 0, bytesToRead);
                    // First byte is slave address
                    UpdateField("Slave Address", 0, Convert.ToString(response[0]), Slave_grid);
                    // Second byte is function code
                    UpdateField("Function", 1, Convert.ToString(response[1]), Slave_grid);
                    // Third byte is byte count
                    UpdateField("Byte Count", 2, Convert.ToString(response[2]), Slave_grid);
                    // Following bytes are the actual data
                    for (int i = 3; i < bytesToRead - 2; i += 2)
                    {
                        string highByte = "Data Hi 40" + ((byte)Convert.ToInt16(Master_grid.Rows[3].Cells["value"].Value) + i / 2).ToString().PadLeft(3, '0') ;
                        string lowByte = "Data Lo 40" + ((byte)Convert.ToInt16(Master_grid.Rows[3].Cells["value"].Value) + i / 2).ToString().PadLeft(3, '0');
                        
                        UpdateField(highByte, i , Convert.ToString(response[i]), Slave_grid);
                        UpdateField(lowByte, i , Convert.ToString(response[i + 1]), Slave_grid);
                    }
                    // Last two bytes are CRC
                    UpdateField("Error Check (CRC)", bytesToRead - 1, Convert.ToString(response[bytesToRead] << 8 | response[bytesToRead - 1]), Slave_grid);
                }
            }
            catch (Exception ex)
            {
                // Clear existing fields
                Master_grid.Rows.Clear();
                // Handle communication errors
                _modbusReadTimer.Stop();
                MessageBox.Show($"Communication error: {ex.Message}");
            }
        }
    }
    private void AddField(string fieldName, string defaultValue, DataGridView grid = null)
    {
        string fieldLabel;
        string fieldValue;
        if (grid == Master_grid)
        {
            fieldLabel = "Field_Name";
            fieldValue = "Value";
        }
        else 
        {
            fieldLabel = "Field_Name_S";
            fieldValue = "Value_S";
        }
        int rowIndex = grid.Rows.Add();
        grid.Rows[rowIndex].Cells[fieldLabel].Value = fieldName;
        grid.Rows[rowIndex].Cells[fieldValue].Value = defaultValue;
    }
    private void UpdateField(string fieldName, int rowIndex, string Value, DataGridView grid = null)
    {
        string fieldLabel;
        string fieldValue;
        if (grid == Master_grid)
        {
            fieldLabel = "Field_Name";
            fieldValue = "Value";
        }
        else 
        {
            fieldLabel = "Field_Name_S";
            fieldValue = "Value_S";
        }
        grid.Rows[rowIndex].Cells[fieldLabel].Value = fieldName;
        grid.Rows[rowIndex].Cells[fieldValue].Value = Value;
    }
    private void InitializeModbusFunctions()
    {
        FunctionSelectcb.Items.Add("Read Holding Registers (03)");
        // Add other Modbus functions latter
    }
    private void COMPort_Open_Click(object sender, EventArgs e)
    {
        // Open serial port
        _serialPort.PortName = COMPort_Comselect.Text;
        _serialPort.BaudRate = Convert.ToInt32(COMPort_Baudrate.Text);
        _serialPort.Open();
        // config status
        COMPort_Status.BackColor = System.Drawing.Color.Green;
        // enable functioning
        Master_grid.Enabled = true;
        Slave_grid.Enabled = true;
        Function_gb.Enabled = true;
        COMPort_Close.Enabled = true;
        // disable COM selector
        COMPort_Open.Enabled = false;
        COMPort_Baudrate.Enabled = false;
        COMPort_Comselect.Enabled = false;
    }
    private void COMPort_Close_Click(object sender, EventArgs e)
    {
        // Stop timer
        _modbusReadTimer.Stop();
        // Close serial port
        _serialPort.Close();
        // config status
        COMPort_Status.BackColor = System.Drawing.Color.Red;
        // disable functioning
        Master_grid.Enabled = false;
        Slave_grid.Enabled = false;
        Function_gb.Enabled = false;
        COMPort_Close.Enabled = false;
        // enable COM selector
        COMPort_Baudrate.Enabled = true;
        COMPort_Comselect.Enabled = true;
        COMPort_Open.Enabled = true;
        // Clear existing fields on table
        Master_grid.Rows.Clear();
        Slave_grid.Rows.Clear();
    }
    
    private void FunctionSend_bt_Click(object sender, EventArgs e)
    {
        string selectedFunction = FunctionSelectcb.SelectedItem.ToString();
        byte[] modbusFrame;
        DataGridViewRowCollection indexs = Master_grid.Rows;
        
        switch (selectedFunction)
        {
            case "Read Holding Registers (03)":
                modbusFrame = new byte[8] {
                    (byte)Convert.ToInt16(Master_grid.Rows[0].Cells["Value"].Value.ToString()),   // Slave address
                    (byte)Convert.ToInt16(Master_grid.Rows[1].Cells["Value"].Value.ToString()),   // Function code
                    (byte)Convert.ToInt16(Master_grid.Rows[2].Cells["Value"].Value.ToString()),   // Start address Hi
                    (byte)Convert.ToInt16(Master_grid.Rows[3].Cells["Value"].Value.ToString()),   // Start address Lo
                    (byte)Convert.ToInt16(Master_grid.Rows[4].Cells["Value"].Value.ToString()),   // Number of Points Hi
                    (byte)Convert.ToInt16(Master_grid.Rows[5].Cells["Value"].Value.ToString()),   // Number of Points Lo
                    0x00,                                                       // Error check placeholder Lo
                    0x00                                                        // Error check placeholder Hi
                };
                if ((string)Master_grid.Rows[6].Cells["value"].Value == "Yes") // Check for CRC field
                {
                    ushort crc = CRC16_Generate(modbusFrame, 6);
                    modbusFrame[6] = (byte)(crc & 0xFF);        // Low byte
                    modbusFrame[7] = (byte)(crc >> 8);          // High byte
                }
                WakeSlave();
                // _serialPort.Write(modbusFrame, 0, 8);
                _modbusReadTimer.Start();
                break;
            // Add other functions latter
            
            default: 
                break;
        }
    }
    private void WakeSlave()
    {
        Slave_grid.Rows.Clear();
        int bytesToRead = 5 + (Convert.ToByte(Master_grid.Rows[5].Cells["value"].Value) * 2);
        
        // First byte is slave address
        AddField("Slave Address", Convert.ToString(Master_grid.Rows[0].Cells["Value"].Value), Slave_grid);
        // Second byte is function code
        AddField("Function", Convert.ToString(Master_grid.Rows[1].Cells["value"].Value), Slave_grid);
        // Third byte is byte count
        AddField("Byte Count", Convert.ToString(bytesToRead - 5), Slave_grid);
        // Following bytes are the actual data
        for (int i = 3; i < bytesToRead - 2; i += 2)
        {
            string highByte = "Data Hi 40" + ((byte)Convert.ToInt16(Master_grid.Rows[3].Cells["value"].Value) + i / 2).ToString().PadLeft(3, '0') ;
            string lowByte = "Data Lo 40" + ((byte)Convert.ToInt16(Master_grid.Rows[3].Cells["value"].Value) + i /2).ToString().PadLeft(3, '0');

            AddField(highByte, "0", Slave_grid);
            AddField(lowByte, "0", Slave_grid);
        }
        // Last two bytes are CRC
        AddField("Error Check (CRC)", "0", Slave_grid);
    }
    private ushort CRC16_Generate(byte[] message, ushort dataLength)
    {
        byte crcHigh = 0xFF; // high CRC byte initialized
        byte crcLow = 0xFF;  // low CRC byte initialized
    
        int index;  // will index into CRC lookup table
    
        // Pass through message buffer
        while (dataLength-- > 0)
        {
            index = crcHigh ^ message[dataLength];
            crcHigh = (byte)(crcLow ^ auchCRCHi[index]);
            crcLow = auchCRCLo[index];
        }
    
        return (ushort)((crcHigh << 8) | crcLow);
    }
    
    private static readonly byte[] auchCRCHi = {
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
        0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
        0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 
        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
        0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 
        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 
        0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40};
    
    private static readonly byte[] auchCRCLo = {
        0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 
        0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 
        0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 
        0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 
        0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4, 
        0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3, 
        0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 
        0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 
        0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 
        0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29, 
        0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED, 
        0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26, 
        0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 
        0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 
        0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 
        0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 
        0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 
        0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5, 
        0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 
        0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 
        0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 
        0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 
        0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B, 
        0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C, 
        0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 
        0x43, 0x83, 0x41, 0x81, 0x80, 0x40
    };
    
}