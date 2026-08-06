import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/car_assignment.dart';
import 'base_provider.dart';

class CarAssignmentProvider extends BaseProvider<CarAssignment> {
  CarAssignmentProvider() : super('CarAssignments');

  @override
  CarAssignment fromJson(dynamic data) =>
      CarAssignment.fromJson(data as Map<String, dynamic>);

  Future<CarAssignment> close(int id) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$id/Close'),
      headers: createHeaders(),
    );

    validateResponse(response);

    return fromJson(jsonDecode(response.body));
  }
}
