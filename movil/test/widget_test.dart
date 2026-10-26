import 'package:flutter_test/flutter_test.dart';
import 'package:movil/main.dart';

void main() {
  testWidgets('La app abre en la lista de tareas', (WidgetTester tester) async {
    await tester.pumpWidget(const AppGestionTareas());
    expect(find.text('Mis tareas'), findsOneWidget);
  });
}