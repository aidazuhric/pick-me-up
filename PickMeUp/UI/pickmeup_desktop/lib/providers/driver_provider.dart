import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/driver.dart';
import '../models/driver_location.dart';
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

  Future<List<DriverLocation>> readLocations() async {
    final response = await http.get(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Locations'),
      headers: createHeaders(),
    );
    validateResponse(response);

    final decoded = jsonDecode(response.body) as List<dynamic>;

    return decoded
        .map((e) => DriverLocation.fromJson(e as Map<String, dynamic>))
        .toList();
  }
}
