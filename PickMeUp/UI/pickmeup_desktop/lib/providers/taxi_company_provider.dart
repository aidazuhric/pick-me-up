import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/taxi_company.dart';
import 'base_provider.dart';

class TaxiCompanyProvider extends BaseProvider<TaxiCompany> {
  TaxiCompanyProvider() : super('TaxiCompanies');

  @override
  TaxiCompany fromJson(dynamic data) =>
      TaxiCompany.fromJson(data as Map<String, dynamic>);

  Future<TaxiCompany> getMine() async {
    final response = await http.get(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Mine'),
      headers: createHeaders(),
    );

    validateResponse(response);

    return fromJson(jsonDecode(response.body));
  }

  Future<TaxiCompany> updateMine(Map<String, dynamic> request) async {
    final response = await http.put(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Mine'),
      headers: createHeaders(),
      body: jsonEncode(request),
    );

    validateResponse(response);

    return fromJson(jsonDecode(response.body));
  }
}
