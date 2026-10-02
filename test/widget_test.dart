import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:shop/main.dart';

void main() {
  testWidgets('App boots on the splash screen', (WidgetTester tester) async {
    await tester.pumpWidget(const MyApp());
    // The splash screen shows the Stylish logo (as an image, or as a
    // text fallback via errorBuilder if the asset can't be decoded
    // in the test environment).
    expect(find.byType(Image), findsOneWidget);
  });
}
