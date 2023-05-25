﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleCurrencyApp.classes
{
    public class CurrencyLoader
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CurrencyLoader(string baseUrl, string apiKey)
        {
            _baseUrl = baseUrl;
            _apiKey = apiKey;
        }


        private async Task<RatesDto> LoadRatesDataAsync()
        {
            string url = $"{_baseUrl}/latest.json?app_id={_apiKey}";            
            RatesDto result = new();
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<RatesDto>(responseBody, _options) ?? new();
            }
            return result;
        }

        private async Task<Dictionary<string, string>> LoadCurrencyInfoAsync()
        {
            string url = $"{_baseUrl}/currencies.json?app_id={_apiKey}";
            Dictionary<string, string> result = new();
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody, _options) ?? new();
            }
            return result;
        }

        public async Task<CurrencyConverter> LoadCurrencyDataAsync()
        {
            RatesDto rates = await LoadRatesDataAsync();
            Dictionary<string, string> currencyData = await LoadCurrencyInfoAsync();
            Dictionary<string, Currency> currencies = new();
            foreach (var rate in rates.Rates) {
                string currencyName = currencyData[rate.Key];
                Currency currency = new(rate.Key, currencyName, rate.Value);
                currencies.Add(rate.Key, currency);
            }
            CurrencyConverter converter = new(currencies);
            return converter;
        }

    }
}
