import 'package:flutter/material.dart';
import '../widgets/app_scaffold.dart';

class DashboardPage extends StatelessWidget {
  const DashboardPage({super.key, required this.shellChild});
  final Widget shellChild;

  @override
  Widget build(BuildContext context) {
    return AppScaffold(child: shellChild);
  }
}
