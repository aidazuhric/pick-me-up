import 'package:flutter_test/flutter_test.dart';

import 'package:pickmeup_mobile/main.dart';

void main() {
  testWidgets('an unauthenticated launch shows the sign-in screen', (
    WidgetTester tester,
  ) async {
    await tester.pumpWidget(const PickMeUpMobileApp());

    expect(find.text('PickMeUp'), findsOneWidget);
    expect(find.text('Sign in'), findsOneWidget);
    expect(find.text('SIGN IN'), findsOneWidget);
  });
}
