using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System;

namespace MODBUS_Master;

public partial class ModbusController : Form
{
    private SerialPort _serialPort;
    private System.Windows.Forms.Timer _modbusReadTimer;
    private AppOrganizer _dataOrganizer = new AppOrganizer();
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
        switch (functionCode)
        {
            case 03:
                // Read Holding Register
                byte[] data = new byte[8]
                {
                    _dataOrganizer.MasterTableState[0].Value,       // Slave address
                    _dataOrganizer.MasterTableState[1].Value,       // Function code
                    _dataOrganizer.MasterTableState[2].Value,       // start address Hi
                    _dataOrganizer.MasterTableState[3].Value,       // Start address Lo
                    _dataOrganizer.MasterTableState[4].Value,       // Number of point Hi
                    _dataOrganizer.MasterTableState[5].Value,       // Number of point Lo
                    0x00,                                           // CRC Lo
                    0x00                                            // CRC Hi
                };
                if (_dataOrganizer.MasterTableState[6].Value == 1) // if using CRC check
                {
                    (byte crcLo, byte crcHi) = _get_CRC16(data);
                    data[6] = crcLo;
                    data[7] = crcHi;
                }
            
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
        switch (functionCode)
        {
            case 03:
                // Read Holding Register
                int dataLength = _dataOrganizer.SlaveTableState.Count;
                byte[] data = new byte[dataLength];
                if (_serialPort.IsOpen)
                    _serialPort.Read(data, 0, dataLength);
                // Update data
                for (int i = 0; i < dataLength; i++)
                    _dataOrganizer.SlaveTableState[i].Value = data[i];
                // update grid
                _update_grid(_dataOrganizer.SlaveTableState, Slave_grid);
                break;
            default:
                break;
        }
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
                        Oper_Status.Text = "Operation OK";
                        Oper_Status.ForeColor = Color.Green;
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
    private void COMPort_Comselect_DropDown(object? sender, EventArgs e)
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
            _serialPort.PortName = COMPort_Comselect.Text;
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
            COMPort_Comselect.Enabled = false;
            COMPort_Open.Enabled = false;
            // turn on LED
            COMPort_Status.BackColor = Color.Green;
            // setup function for MODBUS
            FunctionSelectcb.Items.AddRange("Read Holding Register (0x03)");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
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
            // Close serial port
            _serialPort.Close();
            // Disable other group
            Master_grid.Enabled = false;
            Slave_grid.Enabled = false;
            Function_gb.Enabled = false;
            COMPort_Close.Enabled = false;
            // Enable start port process
            COMPort_Baudrate.Enabled = true;
            COMPort_Comselect.Enabled = true;
            COMPort_Open.Enabled = true;
            // turn on LED
            COMPort_Status.BackColor = Color.Red;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// Handles the event when the selected index in the function selection combo box changes.
    /// This updates the master table state based on the selected Modbus function and refreshes
    /// the data displayed in the Master Grid.
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An EventArgs that contains the event data.</param>
    private void FunctionSelectcb_SelectedIndexChanged(object? sender, EventArgs e)
    {
        string slectedFunction = FunctionSelectcb.Text;

        switch (slectedFunction)
        {
            case "Read Holding Register (0x03)":
                _dataOrganizer.MasterTableState.Clear();
                _dataOrganizer.MasterTableState.AddRange(new AppOrganizer.TableData() { Field = "Address", Value = 0 }, 
                                                                new AppOrganizer.TableData() { Field = "Function Code", Value = 03 },
                                                                new AppOrganizer.TableData() { Field = "Address Data Hi", Value = 0 },
                                                                new AppOrganizer.TableData() { Field = "Address Data Lo", Value = 0 },
                                                                new AppOrganizer.TableData() { Field = "Number of Registers Hi", Value = 0 },
                                                                new AppOrganizer.TableData() { Field = "Number of Registers Lo", Value = 0 },
                                                                new AppOrganizer.TableData() { Field = "CRC", Value = 1 });
                Master_grid.DataSource = _dataOrganizer.MasterTableState;
                break;
            default:
                break;
        }
    }

    /// Event handler for the Send button click event.
    /// Handles the sending of selected MODBUS function and initializes the slave table state.
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the button click event.</param>
    private void Send_bt_Click(object? sender, EventArgs e)
    {
        string slectedFunction = FunctionSelectcb.Text;
        switch (slectedFunction)
        {
            case "Read Holding Register (0x03)":
                _dataOrganizer.SlaveTableState.Clear();
                _dataOrganizer.SlaveTableState.AddRange(new AppOrganizer.TableData() { Field = "Address", Value = 0 }, 
                    new AppOrganizer.TableData() { Field = "Function Code", Value = 0 },
                    new AppOrganizer.TableData() { Field = "Bytes count", Value = 0 });
                int data = _dataOrganizer.MasterTableState[5].Value; // add number of registers x2
                int start = _dataOrganizer.MasterTableState[3].Value;

                for (int i = 0; i < data * 2; i+=2)
                {
                    string dataHi = "40" + (start + i / 2 + 1).ToString().PadLeft(3, '0');
                    _dataOrganizer.SlaveTableState.Add(new AppOrganizer.TableData() { Field = $"Address Data Hi {dataHi}", Value = 0 });
                    string dataLo = "40" + (start + i  / 2 + 1).ToString().PadLeft(3, '0');
                    _dataOrganizer.SlaveTableState.Add(new AppOrganizer.TableData() { Field = $"Address Data Lo {dataLo}", Value = 0 });
                }

                _dataOrganizer.SlaveTableState.Add(new AppOrganizer.TableData() { Field = "CRC Lo", Value = 0 });
                _dataOrganizer.SlaveTableState.Add(new AppOrganizer.TableData() { Field = "CRC Hi", Value = 0 });
                // update grid
                Slave_grid.DataSource = null;
                _update_grid(_dataOrganizer.SlaveTableState, Slave_grid);
                // start timer
                _modbusReadTimer.Start();
                break;
            default:
                break;
        }
    }
    #endregion
    
    #region Functionallity
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
    }
    private void _auto_select_baudrate(object? sender, EventArgs e)
    {
        COMPort_Baudrate.SelectedIndex = 0;
    }
    private void _get_COM_Ports()
    { 
        // Clear existed item
        COMPort_Comselect.Items.Clear();
        // Get available ports
        string[] ports = SerialPort.GetPortNames();
        if (ports.Length > 0)
        {
            foreach (string port in ports)
            {
                COMPort_Comselect.Items.Add(port);
            }
        }
        else
        {
            // Handle case when no ports are available
            COMPort_Comselect.Items.Add("No ports available");
        }

        // Select first item in list
        if (COMPort_Comselect.Items.Count <= 1)
            return;
        COMPort_Comselect.SelectedIndex = 0;
    }
    private void Master_Grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (sender is DataGridView gridView && e.RowIndex >= 0)
        {
            byte newValue = 0;
            if (byte.TryParse(gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString(), out byte parsedValue))
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
        public byte Value { get; set; }
    }

    public AppOrganizer()
    {
        // Initialize collections
        MasterTableState = new List<TableData>();
        SlaveTableState = new List<TableData>();
    }
}
