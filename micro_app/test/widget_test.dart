// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:micro_app/main.dart';

void main() {
  testWidgets('Shows Login by default when no token is stored', (WidgetTester tester) async {
    SharedPreferences.setMockInitialValues({});

    await tester.pumpWidget(const MyApp());
    // initial frame may show a loader; settle the FutureBuilder
    await tester.pumpAndSettle();

    expect(find.text('Login'), findsOneWidget);
  });
}
