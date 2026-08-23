import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/taxi_company.dart';
import '../models/taxi_company_details.dart';
import 'base_provider.dart';

class TaxiCompanyProvider extends BaseProvider<TaxiCompany> {
  TaxiCompanyProvider() : super('TaxiCompanies');

  @override
  TaxiCompany fromJson(dynamic data) =>
      TaxiCompany.fromJson(data as Map<String, dynamic>);

  Future<TaxiCompanyDetails> readDetails(int id) async {
    final response = await http.get(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$id/Details'),
      headers: createHeaders(),
    );
    validateResponse(response);

    return TaxiCompanyDetails.fromJson(jsonDecode(response.body));
  }
}
