import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';

import 'package:pickmeup_desktop/providers/auth_provider.dart';
import 'package:pickmeup_desktop/screens/reference/reference_data_hub.dart';
import 'package:pickmeup_desktop/screens/reference/reference_table.dart';

Future<void> _settle(WidgetTester tester) async {
  for (var i = 0; i < 8; i++) {
    await tester.runAsync(
        () => Future.delayed(const Duration(milliseconds: 300)));
    await tester.pump();
  }
}

Future<void> _pumpHub(WidgetTester tester) async {
  await tester.binding.setSurfaceSize(const Size(1440, 900));

  final auth = AuthProvider();
  await tester.runAsync(() => auth.login('admin1', 'Test123'));

  await tester.pumpWidget(
    ChangeNotifierProvider<AuthProvider>.value(
      value: auth,
      child: const MaterialApp(
        home: Scaffold(
          body: Padding(padding: EdgeInsets.all(32), child: ReferenceDataHub()),
        ),
      ),
    ),
  );

  await _settle(tester);
}

void main() {
  setUpAll(() => HttpOverrides.global = null);

  testWidgets('every reference table is reachable and lists live rows',
      (tester) async {
    await _pumpHub(tester);

    for (final table in referenceTables) {
      expect(find.text(table.title), findsWidgets,
          reason: '${table.title} chip is missing');
    }

    expect(find.text('Mostar'), findsOneWidget);
    expect(find.text('Sarajevo'), findsOneWidget);
    expect(find.text('88000'), findsOneWidget);
    expect(find.text('5 records'), findsOneWidget);

    for (final table in referenceTables.skip(1)) {
      await tester.tap(find.widgetWithText(ChoiceChip, table.title));
      await _settle(tester);
      expect(find.text(table.nameLabel), findsWidgets,
          reason: '${table.title} did not render its column header');
    }

    await tester.tap(find.widgetWithText(ChoiceChip, 'Order statuses'));
    await _settle(tester);
    expect(find.text('Pending'), findsOneWidget);
    expect(find.text('Completed'), findsOneWidget);
    expect(find.text('Display order'), findsWidgets);
  });

  testWidgets('the create form validates below the control', (tester) async {
    await _pumpHub(tester);

    await tester.tap(find.text('ADD NEW'));
    await _settle(tester);

    expect(find.text('New city'), findsOneWidget);
    expect(find.text('City name'), findsWidgets);
    expect(find.text('Postal code'), findsWidgets);

    await tester.tap(find.text('SAVE'));
    await tester.pump();

    expect(find.text('City name is required.'), findsOneWidget);

    await tester.enterText(
        find.byType(TextFormField).first, 'x' * 101);
    await tester.pump();
    expect(find.text('City name must be at most 100 characters.'), findsOneWidget);

    await tester.tap(find.byTooltip('Back'));
    await _settle(tester);
    expect(find.text('New city'), findsNothing);
  });

  testWidgets('deleting a referenced city shows the backend message',
      (tester) async {
    await _pumpHub(tester);

    expect(find.text('Mostar'), findsOneWidget);

    final deleteButtons = find.byTooltip('Delete');
    expect(deleteButtons, findsWidgets);

    await tester.tap(deleteButtons.first);
    await _settle(tester);

    expect(find.text('Remove this city?'), findsOneWidget);
    expect(find.textContaining('cannot be removed'), findsOneWidget);

    await tester.tap(find.text('DELETE'));
    await _settle(tester);

    expect(
      find.text(
        'This city is used by existing records and cannot be deleted. Deactivate it instead.',
      ),
      findsOneWidget,
    );
  });
}
