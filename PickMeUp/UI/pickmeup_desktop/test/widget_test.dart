import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:pickmeup_desktop/main.dart';

void main() {
  testWidgets('unauthenticated start shows the sign in form', (tester) async {
    await tester.pumpWidget(const PickMeUpDesktopApp());

    expect(find.text('Sign in'), findsOneWidget);
    expect(find.widgetWithText(TextFormField, 'Username'), findsOneWidget);
    expect(find.widgetWithText(TextFormField, 'Password'), findsOneWidget);
    expect(find.text('SIGN IN'), findsOneWidget);
  });

  testWidgets('submitting an empty form shows validation messages',
      (tester) async {
    await tester.pumpWidget(const PickMeUpDesktopApp());

    await tester.tap(find.text('SIGN IN'));
    await tester.pump();

    expect(find.text('Enter your username.'), findsOneWidget);
    expect(find.text('Enter your password.'), findsOneWidget);
  });
}
