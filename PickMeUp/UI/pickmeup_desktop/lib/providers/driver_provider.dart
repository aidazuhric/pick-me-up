import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/driver.dart';
import 'base_provider.dart';

class DriverProvider extends BaseProvider<Driver> {
  DriverProvider() : super('Drivers');

  @override
  Driver fromJson(dynamic data) => Driver.fromJson(data as Map<String, dynamic>);

  Future<Driver> resign(int id) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$id/Resign'),
      headers: createHeaders(),
    );

    validateResponse(response);

    return fromJson(jsonDecode(response.body));
  }
}
