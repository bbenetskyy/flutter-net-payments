import 'package:flutter/material.dart';

class ResponsiveBuilder extends StatelessWidget {
  const ResponsiveBuilder({super.key, required this.desktop, required this.mobile});
  final Widget desktop;
  final Widget mobile;

  static bool isDesktop(BuildContext context) => MediaQuery.of(context).size.width >= 900;

  @override
  Widget build(BuildContext context) {
    final isDesk = isDesktop(context);
    return isDesk ? desktop : mobile;
  }
}
