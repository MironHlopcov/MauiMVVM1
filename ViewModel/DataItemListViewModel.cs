using MauiMVVM.Controls;
using MauiMVVM.Service;
using MauiMVVM.View;
using System.Collections.ObjectModel;


namespace MauiMVVM.ViewModel
{
    public class DataItemListViewModel : BaseViewModel
    {

        public DataItemService DataItemService { get; set; }

        private List<DataItemViewModel> dataItems { get; set; } = new();
        public ObservableCollection<DataItemViewModel> DataItems { get; set; }

        public INavigation Navigation { get; set; }
        public Command GetDataItemsComand { get; }
        public Command GetDataItemsDetailPageComand { get; }


        string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText == value)
                    return;
                searchText = value;
                if (string.IsNullOrWhiteSpace(searchText))
                    SearchDataItems();
                 OnPropertyChanged(); //если текст не меняется из кода применять нет смысла

            }
        }

        DataItemViewModel selectedDataItem;
        public DataItemViewModel SelectedDataItem
        {
            get => selectedDataItem;
            set
            {
                if (selectedDataItem == value)
                    return;
                selectedDataItem = value;
            }
        }

        public DataItemListViewModel()
        {
            DataItems = new ObservableCollection<DataItemViewModel>();
            GetDataItemsComand = new Command(async () => await GetDataItemAsync());
            SearchDataItemsComand = new Command(SearchDataItems);
            FilterDataItemsComand = new Command(GetFilterResult);
            CleanFilterDataItemsComand = new Command(CleanFilter);
            GetDataItemsDetailPageComand = new Command(GetDataItemsDetailPage);
        }

        async void GetDataItemsDetailPage(object obj)
        {
            await Navigation.PushAsync(new DataItemDetailPage((obj as DataItemViewModel)));
        }

        async Task GetDataItemAsync()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                if (dataItems.Count != 0)
                    dataItems.Clear();
                var dataItemsFromDb = await DataItemService.GetDataItems();
                for (int i = 0; i < 10; i++)
                {
                    foreach (var data in dataItemsFromDb)
                    {
                        dataItems.Add(new DataItemViewModel(data)
                        {
                            DataItemListViewModel = this
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to get DataItems: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Unable to get DataItems: {ex.Message}", "Ok");
            }
            finally
            {
                foreach (var data in dataItems)
                {
                    DataItems.Add(data);

                }
                IsBusy = false;
                InitializeFilter();
            }
        }



        private List<GroupItem> filtredFilds=new();
        public List<GroupItem> FiltredFilds
        {
            get => filtredFilds;
            set
            {
                if (filtredFilds == value)
                    return;
                filtredFilds = value;
                OnPropertyChanged(); //если текст не меняется из кода применять нет смысла
            }
        }

        public Command SearchDataItemsComand { get; }
        public Command FilterDataItemsComand { get; }
        public Command CleanFilterDataItemsComand { get; }
        void SearchDataItems()
        {
            //if (string.IsNullOrEmpty(searchText))
            //{
            //    DataItems.Clear();
            //    dataItems.ForEach(d => DataItems.Add(d));
            //}
            //else
            //{
            //    DataItems.Clear();
            //    var result = dataItems.Where(d => d.Name.Contains(searchText)).ToList();
            //    result.ForEach(d => DataItems.Add(d));
            //}
            GetFilterResult();
        }
        private void GetFilterResult()
        {
            List<DataItemViewModel> result = new();
            result.AddRange(dataItems);
            if (string.IsNullOrEmpty(searchText))
            {
                dataItems.ForEach(d => DataItems.Add(d));
            }
            else
            {
                result = result.Where(d => d.Name.Contains(searchText)).ToList();
            }

            foreach (var exoFilterItem in filtredFilds)
            {
                if (exoFilterItem.Key == "Name")
                {
                    var selectedValues = exoFilterItem.Items.Where(x=>x.Value==true).Select(x=>x.Key).ToList();
                    if(selectedValues.Count!=0)
                    result = result.Where(d =>selectedValues.Contains(d.Name)).ToList();
                }
            }
            DataItems.Clear();
            result.ForEach(d => DataItems.Add(d));
        }
        private void CleanFilter()
        {
            DataItems.Clear();
            SearchText = "";
            filtredFilds.ForEach(x=>x.Value = false);
            dataItems.ForEach(d => DataItems.Add(d));
        }
        void InitializeFilter()
        {
            var names = new List<Item>();
            dataItems.GroupBy(x=>x.Name).Select(x=>x.First()).ToList()
                .ForEach(x => names.Add(new Item { Key = x.Name }));

            var names2 = new List<Item>();
            dataItems.GroupBy(x => x.Name).Select(x => x.First()).ToList()
                .ForEach(x => names2.Add(new Item { Key = x.Name }));

            var dFiltredFilds = new List<GroupItem>();
            dFiltredFilds.Add( new() { Key= "Name", Items= names.ToArray() });
            FiltredFilds = dFiltredFilds;
        }

    }
}
