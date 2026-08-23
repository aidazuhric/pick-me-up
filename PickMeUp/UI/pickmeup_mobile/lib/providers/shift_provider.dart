import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/shift.dart';
import 'base_provider.dart';

class ShiftProvider extends BaseProvider<Shift> {
  ShiftProvider() : super('Shifts');

  @override
  Shift fromJson(dynamic data) => Shift.fromJson(data as Map<String, dynamic>);

  Future<Shift?> readOpenShift() async {
    final result = await get(
      filter: {'OngoingOnly': true, 'Page': 1, 'PageSize': 1},
    );

    final items = result.items ?? const <Shift>[];

    return items.isEmpty ? null : items.first;
  }

  Future<Shift> start() => _post('Start');

  Future<Shift> stop(int shiftId) => _post('$shiftId/Stop');

  Future<Shift> startBreak(int shiftId) => _post('$shiftId/Break/Start');

  Future<Shift> stopBreak(int shiftId) => _post('$shiftId/Break/Stop');

  Future<Shift> _post(String path) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$path'),
      headers: createHeaders(),
    );
    validateResponse(response);

    return Shift.fromJson(jsonDecode(response.body));
  }
}
