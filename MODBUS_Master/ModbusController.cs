using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System;

namespace MODBUS_Master;

public partial class ModbusController : Form
{
    private readonly SerialPort _serialPort;
    private readonly System.Windows.Forms.Timer _modbusReadTimer;
    private readonly AppOrganizer _dataOrganizer = new();
    public ModbusController()
    {
        // Initialize COM Ports
        _serialPort = new SerialPort();
        // Initialize timer
        _modbusReadTimer = new System.Windows.Forms.Timer();
        _modbusReadTimer.Interval = 1000;
        _modbusReadTimer.Tick += Timer_Tick;
        
        InitializeComponent();
        
        // disable functioning
        Master_grid.Enabled = false;
        Slave_grid.Enabled = false;
        Function_gb.Enabled = false;
        COMPort_Close.Enabled = false;
    }

    #region Main Protocol

    /// <summary>
    /// Sends data over the serial port using a specified function code within the MODBUS communication protocol.
    /// </summary>
    /// <param name="functionCode">The MODBUS function code specifying the action to perform (e.g., reading holding registers).</param>
    private void Send_data(int functionCode)
    {
        byte[] data;
        switch (functionCode)
        { 
            case 03:
                // Read Holding Register
                data = 
                [
                    (byte)_dataOrganizer.MasterTableState[0].Value,       // Slave address
                    (byte)_dataOrganizer.MasterTableState[1].Value,       // Function code
                    (byte)(_dataOrganizer.MasterTableState[2].Value >> 8),       // start address Hi
                    (byte)(_dataOrganizer.MasterTableState[2].Value & 0xFF),       // Start address Lo
                    (byte)(_dataOrganizer.MasterTableState[3].Value >> 8),       // Number of point Hi
                    (byte)(_dataOrganizer.MasterTableState[3].Value & 0xFF),       // Number of point Lo
                    0x00,                                           // CRC Lo
                    0x00                                            // CRC Hi
                ];
                if (_dataOrganizer.MasterTableState[4].Value == 1) // if using CRC check
                {
                    var( crcLo,  crcHi) = _get_CRC16(data);
                    data[6] = crcLo;
                    data[7] = crcHi;
                }
                Sender_text.Text = string.Join(" ", data.Select(d => "0x" + d.ToString("X2")));
                
                if (_serialPort.IsOpen)
                    _serialPort.Write(data, 0, data.Length);
                break;
            case 06:
                // Preset Single Register
                data = 
                [
                    (byte)_dataOrganizer.MasterTableState[0].Value,       // Slave address
                    (byte)_dataOrganizer.MasterTableState[1].Value,       // Function code
                    (byte)(_dataOrganizer.MasterTableState[2].Value >> 8),       // start address Hi
                    (byte)(_dataOrganizer.MasterTableState[2].Value & 0xFF),       // Start address Lo
                    (byte)(_dataOrganizer.MasterTableState[3].Value >> 8),       // Preset Value Hi
                    (byte)(_dataOrganizer.MasterTableState[3].Value & 0xFF),       // Preset Value Lo
                    0x00,                                           // CRC Lo
                    0x00                                            // CRC Hi
                ];
                if (_dataOrganizer.MasterTableState[4].Value == 1) // if using CRC check
                {
                    var( crcLo,  crcHi) = _get_CRC16(data);
                    data[6] = crcLo;
                    data[7] = crcHi;
                }
                Sender_text.Text = string.Join(" ", data.Select(d => "0x" + d.ToString("X2")));
                
                if (_serialPort.IsOpen)
                    _serialPort.Write(data, 0, data.Length);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Reads data from the serial port using a specified function code within the MODBUS communication protocol and updates the slave table state and grid view.
    /// </summary>
    /// <param name="functionCode">The MODBUS function code specifying the action to perform (e.g., reading holding registers).</param>
    private void Read_data(int functionCode)
    {
        byte[] data = new byte[2];
        // Create a List
        List<byte> response = new List<byte>();
        switch (functionCode)
        { 
            case 03:
                // Read Holding Register
                if (_serialPort.IsOpen)
                {
                    // Update data
                    _serialPort.Read(data, 0, 1);
                    _dataOrganizer.SlaveTableState[0].Value = data[0];      // Address echo
                    response.Add(data[0]);
                    _serialPort.Read(data, 0, 1);
                    _dataOrganizer.SlaveTableState[1].Value = data[0];      // Function code echo
                    response.Add(data[0]);
                    _serialPort.Read(data, 0, 1);
                    _dataOrganizer.SlaveTableState[2].Value = data[0];      // Byte count
                    response.Add(data[0]);
                    // read data
                    for (var i = 0; i < _dataOrganizer.SlaveTableState[2].Value / 2; i++)
                    {                    
                        _serialPort.Read(data, 0, 2);
                        _dataOrganizer.SlaveTableState[i + 3].Value = (ushort)(data[0] << 8 | data[1]);
                        response.Add(data[0]);
                        response.Add(data[1]);
                    }
                    // CRC check
                    _serialPort.Read(data, 0, 2);
                    _dataOrganizer.SlaveTableState[^1].Value = 
                        (ushort)(data[1] << 8 | data[0]);      // CRC Hi | CRC Lo
                    response.Add(data[0]);
                    response.Add(data[1]);
                }
                break;
            case 06:
                // Preset Single Register
                if (_serialPort.IsOpen)
                {
                    // Update data
                    _serialPort.Read(data, 0, 1);
                    _dataOrganizer.SlaveTableState[0].Value = data[0];      // Address echo
                    response.Add(data[0]);
                    _serialPort.Read(data, 0, 1);
                    _dataOrganizer.SlaveTableState[1].Value = data[0];      // Function code echo
                    response.Add(data[0]);
                    _serialPort.Read(data, 0, 2);
                    _dataOrganizer.SlaveTableState[2].Value = (ushort)(data[0] << 8 | data[1]);      // Data Address echo
                    response.Add(data[0]);
                    response.Add(data[1]);
                    _serialPort.Read(data, 0, 2);
                    _dataOrganizer.SlaveTableState[3].Value = (ushort)(data[0] << 8 | data[1]);      // Preset value echo
                    response.Add(data[0]);
                    response.Add(data[1]);
                    _serialPort.Read(data, 0, 2);
                    _dataOrganizer.SlaveTableState[4].Value = (ushort)(data[1] << 8 | data[1]);      // CRC Value
                    response.Add(data[0]);
                    response.Add(data[1]);
                }
                break;
            default:
                break;
        }
        // check CRC
        // foreach (var d in response)
        //     Console.Write(" " + d.ToString("X2"));       // Debug
        // Console.WriteLine();
        if (ValidateSlaveResponseCrc(response.ToArray()))
        {
            const string status = "Operation OK";
            Oper_Status.Text = status;
            Oper_Status.ForeColor = Color.Green;
        }
        else
        {
            const string status = "CRC Invalid";
            Oper_Status.Text = status;
            Oper_Status.ForeColor = Color.Red;
        }
        // update grid
        _update_grid(_dataOrganizer.SlaveTableState, Slave_grid);
    }

    /// <summary>
    /// Handles the tick event of the MODBUS read timer, clearing buffers and managing
    /// MODBUS commands based on the function code from the master table state.
    /// </summary>
    /// <param name="sender">The source of the event, typically the timer initiating the tick.</param>
    /// <param name="e">The event data associated with the timer tick.</param>
    private async void Timer_Tick(object? sender, EventArgs e)
    {
        await Task.Run(() =>
        {
            try
            {
                // clear data
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                // get function code
                switch (_dataOrganizer.MasterTableState[1].Value)
                {
                    case 03:
                        // Send command: Read Holding Register
                        Send_data(03);
                        // Read data
                        Read_data(03);
                        break;
                    case 06:
                        // Send command: Read Holding Register
                        Send_data(06);
                        // Read data
                        Read_data(06);
                        break;
                }
            }
            catch (Exception exception)
            {
                Oper_Status.Text = exception.Message;
                Oper_Status.ForeColor = Color.Red;
            }
        });
    }
    #endregion
    
    #region Interface_interaction
    /// <summary>
    /// Event handler to update the list of available COM ports.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An instance containing the event data.</param>
    private void COMPort_ComSelect_DropDown(object? sender, EventArgs e)
    {
        // Update COM Ports list
        _get_COM_Ports();
    }

    /// <summary>
    /// Event handler to initialize and open the serial port.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An instance containing the event data.</param>
    private void COMPort_Open_Click(object? sender, EventArgs e)
    {
        try
        {
            // setup serial port
            _serialPort.PortName = COMPort_ComSelect.Text;
            _serialPort.BaudRate = Convert.ToInt32(COMPort_Baudrate.Text);
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.Two;
            _serialPort.WriteTimeout = 500;
            _serialPort.ReadTimeout = 500;
            _serialPort.Open();
            
            // Enable other group
            Master_grid.Enabled = true;
            Slave_grid.Enabled = true;
            Function_gb.Enabled = true;
            COMPort_Close.Enabled = true;
            // Disable start port process to prevent error
            COMPort_Baudrate.Enabled = false;
            COMPort_ComSelect.Enabled = false;
            COMPort_Open.Enabled = false;
            // turn on LED
            COMPort_Status.BackColor = Color.Green;
            // setup function for MODBUS
            FunctionSelectcb.Items.Clear();
            FunctionSelectcb.Items.AddRange(
                "Read Holding Register (0x03)",
                "Preset Single Register (0x06)");
        }
        catch (Exception ex)
        {
            Sender_text.Text = ex.Message;
            Sender_text.ForeColor = Color.Red;
        }
    }

    /// <summary>
    /// Handles the event triggered by clicking the Close COM Port button.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void COMPort_Close_Click(object? sender, EventArgs e)
    {
        try
        {
            // Stop timer
            _modbusReadTimer.Stop();
            // Close serial port
            _serialPort.Close();
            // Disable other group
            Master_grid.Enabled = false;
            Slave_grid.Enabled = false;
            Function_gb.Enabled = false;
            COMPort_Close.Enabled = false;
            // CLear text
            Master_grid.DataSource = null;
            Slave_grid.DataSource = null;
            FunctionSelectcb.Text = "";
            Oper_Status.Text = "";
            Sender_text.Text = "";
            // Enable start port process
            COMPort_Baudrate.Enabled = true;
            COMPort_ComSelect.Enabled = true;
            COMPort_Open.Enabled = true;
            // turn on LED
            COMPort_Status.BackColor = Color.Red;
        }
        catch (Exception ex)
        {
            Sender_text.Text = ex.Message;
            Sender_text.ForeColor = Color.Red;
        }
    }

    /// Handles the event when the selected index in the function selection combo box changes.
    /// This updates the master table state based on the selected Modbus function and refreshes
    /// the data displayed in the Master Grid.
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An EventArgs that contains the event data.</param>
    private void FunctionSelectcb_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _modbusReadTimer.Stop();
        Slave_grid.DataSource = null;
        string slectedFunction = FunctionSelectcb.Text;
        _dataOrganizer.MasterTableState.Clear();
        switch (slectedFunction)
        {
            case "Read Holding Register (0x03)":
                _dataOrganizer.MasterTableState.AddRange(new AppOrganizer.TableData() { Field = "Address", Value = 0 },
                                                                new AppOrganizer.TableData() { Field = "Function Code", Value = 03 },
                                                                new AppOrganizer.TableData() { Field = "Data Address", Value = 0 },
                                                                new AppOrganizer.TableData() { Field = "Number of Registers", Value = 0 },
                                                                new AppOrganizer.TableData() { Field = "CRC", Value = 1 });
                break;
            case "Preset Single Register (0x06)":
                _dataOrganizer.MasterTableState.AddRange(new AppOrganizer.TableData() { Field = "Address", Value = 0 },
                    new AppOrganizer.TableData() { Field = "Function Code", Value = 06 },
                    new AppOrganizer.TableData() { Field = "Data Address", Value = 0 },
                    new AppOrganizer.TableData() { Field = "Preset value", Value = 0 },
                    new AppOrganizer.TableData() { Field = "CRC", Value = 1 });
                break;
            default:
                break;
        }
        Master_grid.DataSource = _dataOrganizer.MasterTableState;
        Master_grid.Refresh();
    }

    /// Event handler for the Send button click event.
    /// Handles the sending of selected MODBUS function and initializes the slave table state.
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the button click event.</param>
    private void Send_bt_Click(object? sender, EventArgs e)
    {
        try
        {
            string slectedFunction = FunctionSelectcb.Text;
            _dataOrganizer.SlaveTableState.Clear();
            // construct frame
            switch (slectedFunction)
            {
                case "Read Holding Register (0x03)":
                    _dataOrganizer.SlaveTableState.AddRange(new AppOrganizer.TableData() { Field = "Address", Value = 0 }, 
                        new AppOrganizer.TableData() { Field = "Function Code", Value = 0 },
                        new AppOrganizer.TableData() { Field = "Bytes count", Value = 0 });
                    int numberData = _dataOrganizer.MasterTableState[3].Value; // add number of registers x2
                    int start = _dataOrganizer.MasterTableState[2].Value;

                    for (int i = 0; i < numberData * 2; i+=2)
                    {
                        string data = "40" + (start + i  / 2 + 1).ToString().PadLeft(3, '0');
                        _dataOrganizer.SlaveTableState.Add(new AppOrganizer.TableData() { Field = $"Data Address {data}", Value = 0 });
                    }
                    _dataOrganizer.SlaveTableState.Add(new AppOrganizer.TableData() { Field = "CRC", Value = 0 });
                    break;
                case "Preset Single Register (0x06)":
                    _dataOrganizer.SlaveTableState.AddRange(new AppOrganizer.TableData() { Field = "Address", Value = 0 },
                        new AppOrganizer.TableData() { Field = "Function Code", Value = 06 },
                        new AppOrganizer.TableData() { Field = "Data Address", Value = 0 },
                        new AppOrganizer.TableData() { Field = "Preset value", Value = 0 },
                        new AppOrganizer.TableData() { Field = "CRC", Value = 1 });
                    break;
                default:
                    break;
            }
            // update grid
            Slave_grid.DataSource = null;
            _update_grid(_dataOrganizer.SlaveTableState, Slave_grid);
            Slave_grid.Refresh();
            // start timer
            _modbusReadTimer.Start();
        }
        catch (Exception ex)
        {
            Oper_Status.Text = ex.Message;
            Oper_Status.ForeColor = Color.Red;
        }
    }
    #endregion
    
    #region Functionallity
    private bool ValidateSlaveResponseCrc(byte[] response)
    {
        if (response.Length < 2)  // Minimum length check
            return false;
        
        // Get the received CRC from last 2 bytes
        byte receivedCrcLow = response[response.Length - 2];
        byte receivedCrcHigh = response[response.Length - 1];
    
        // Calculate CRC for the message (excluding CRC bytes)
        byte[] messageWithoutCrc = response.Take(response.Length - 2).ToArray();
        var (calculatedCrcLow, calculatedCrcHigh) = _get_CRC16(response.ToArray());
    
        // Compare calculated and received CRC
        return (receivedCrcLow == calculatedCrcLow && receivedCrcHigh == calculatedCrcHigh);
    }
    private (byte low, byte high) _get_CRC16(byte[] data)
    {
        ushort crc = 0xFFFF;
    
        for (int pos = 0; pos < data.Length - 2; pos++)
        {
            crc ^= data[pos];
        
            for (int i = 8; i != 0; i--)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc >>= 1;
                    crc ^= 0xA001;
                }
                else
                {
                    crc >>= 1;
                }
            }
        }
    
        return ((byte)(crc & 0xFF), (byte)(crc >> 8));
    }
    private void _update_grid(List<AppOrganizer.TableData> data, DataGridView gridView)
    {
        if (gridView.DataSource == null)
        {
            gridView.DataSource = data;
            return;
        }
        var currentData = (List<AppOrganizer.TableData>)gridView.DataSource;
        // Update values and check if Field names match
        for (int i = 0; i < data.Count && i < currentData.Count; i++)
        {
            if (currentData[i].Value != data[i].Value)
            {
                currentData[i].Value = data[i].Value;
                gridView.InvalidateRow(i);
            }
        }
        // If using direct grid updates
        gridView.Refresh();
    }
    private void _auto_select_baudrate(object? sender, EventArgs e)
    {
        COMPort_Baudrate.SelectedIndex = 0;
    }
    private void _get_COM_Ports()
    { 
        // Clear existed item
        COMPort_ComSelect.Items.Clear();
        // Get available ports
        string[] ports = SerialPort.GetPortNames();
        if (ports.Length > 0)
        {
            foreach (string port in ports)
            {
                COMPort_ComSelect.Items.Add(port);
            }
        }
        else
        {
            // Handle case when no ports are available
            COMPort_ComSelect.Items.Add("No ports available");
        }

        // Select first item in list
        if (COMPort_ComSelect.Items.Count <= 1)
            return;
        COMPort_ComSelect.SelectedIndex = 0;
    }
    private void Master_Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (sender is DataGridView gridView && e.RowIndex >= 0)
        {
            ushort newValue = 0;
            if (ushort.TryParse(gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString(), out ushort parsedValue))
            {
                newValue = parsedValue;
            }

            // Get the field name from the first column of the changed row
            var fieldName = gridView.Rows[e.RowIndex].Cells[0].Value?.ToString();

            // Update the corresponding TableData in MasterTableState
            var tableData = _dataOrganizer.MasterTableState.FirstOrDefault(td => td.Field == fieldName);
            if (tableData != null)
            {
                tableData.Value = newValue;
            }
        }
    }
    #endregion
}

public class AppOrganizer
{
    // Tables state
    public List<TableData> MasterTableState { get; set; }
    public List<TableData> SlaveTableState { get; set; }

    // Helper class for table data
    public class TableData
    {
        public string Field { get; set; }
        public UInt16 Value { get; set; }
    }
    
    public AppOrganizer()
    {
        // Initialize collections
        MasterTableState = new List<TableData>();
        SlaveTableState = new List<TableData>();
        // Inittalize CRC check
    }
}
