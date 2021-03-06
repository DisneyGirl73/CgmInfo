﻿using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using CgmInfo;
using CgmInfo.Commands;
using CgmInfoGui.Traversal;
using CgmInfoGui.ViewModels.Nodes;
using Microsoft.Win32;

namespace CgmInfoGui.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (value != _fileName)
                {
                    _fileName = value;
                    ProcessCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged();
                }
            }
        }

        private List<NodeBase> _metafileNodes;
        public List<NodeBase> MetafileNodes
        {
            get { return _metafileNodes; }
            set
            {
                if (value != _metafileNodes)
                {
                    _metafileNodes = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<NodeBase> _apsNodes;
        public List<NodeBase> APSNodes
        {
            get { return _apsNodes; }
            set
            {
                if (value != _apsNodes)
                {
                    _apsNodes = value;
                    OnPropertyChanged();
                }
            }
        }

        private XDocument _xcfDocument;
        public XDocument XCFDocument
        {
            get { return _xcfDocument; }
            set
            {
                if (value != _xcfDocument)
                {
                    _xcfDocument = value;
                    OnPropertyChanged();
                }
            }
        }

        private MetafileProperties _metafileDescriptor;
        public MetafileProperties MetafileProperties
        {
            get { return _metafileDescriptor; }
            set
            {
                if (value != _metafileDescriptor)
                {
                    _metafileDescriptor = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<HotspotNode> _hotspots;
        public List<HotspotNode> Hotspots
        {
            get { return _hotspots; }
            set
            {
                if (value != _hotspots)
                {
                    _hotspots = value;
                    OnPropertyChanged();
                }
            }
        }

        public DelegateCommand BrowseCommand { get; }
        public DelegateCommand ProcessCommand { get; }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    BrowseCommand.NotifyCanExecuteChanged();
                    ProcessCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public MainViewModel()
        {
            BrowseCommand = new DelegateCommand(Browse, CanBrowse);
            ProcessCommand = new DelegateCommand(Process, CanProcess);
        }

        private bool CanBrowse(object parameter) => !IsBusy;
        private void Browse(object parameter)
        {
            var ofd = new OpenFileDialog
            {
                AddExtension = false,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".cgm",
                Filter = "Computer Graphics Metafile (*.cgm)|*.cgm",
                Multiselect = false,
                RestoreDirectory = true,
                Title = "Select a CGM file",
            };
            if (ofd.ShowDialog() == true)
            {
                FileName = ofd.FileName;
                if (CanProcess(null))
                    Process(null);
            }
        }

        private bool CanProcess(object parameter) => !IsBusy && File.Exists(FileName);
        private async void Process(object parameter)
        {
            IsBusy = true;
            var result = await Task.Run(() =>
            {
                using (var reader = MetafileReader.Create(FileName))
                {
                    var vmVisitor = new ViewModelBuilderVisitor();
                    var metafileContext = new MetafileContext();
                    var apsVisitor = new APSStructureBuilderVisitor();
                    var apsContext = new APSStructureContext();
                    var xcfVisitor = new XCFDocumentBuilderVisitor();
                    var xcfContext = new XCFDocumentContext();
                    var hotspotVisitor = new HotspotBuilderVisitor();
                    var hotspotContext = new HotspotContext();
                    Command command;
                    do
                    {
                        command = reader.Read();
                        if (command != null)
                        {
                            command.Accept(vmVisitor, metafileContext);
                            command.Accept(apsVisitor, apsContext);
                            command.Accept(xcfVisitor, xcfContext);
                            command.Accept(hotspotVisitor, hotspotContext);
                        }
                    } while (command != null);
                    return new
                    {
                        MetafileNodes = metafileContext.RootLevel.ToList(),
                        APSNodes = apsContext.RootLevel.ToList(),
                        XCFDocument = xcfContext.XCF,
                        Hotspots = hotspotContext.RootLevel.OfType<HotspotNode>().ToList(),
                        MetafileProperties = reader.Properties,
                    };
                }
            });
            IsBusy = false;
            MetafileNodes = result.MetafileNodes;
            APSNodes = result.APSNodes;
            XCFDocument = result.XCFDocument;
            Hotspots = result.Hotspots;
            MetafileProperties = result.MetafileProperties;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
