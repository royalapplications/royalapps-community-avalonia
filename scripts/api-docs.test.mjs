import assert from "node:assert/strict";
import path from "node:path";
import { fileURLToPath } from "node:url";
import test from "node:test";
import { collectPublicApiSurface, getDisplayTypeName, renderTypeExpression } from "./generate-api-docs.mjs";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const common = "RoyalApps.Community.Avalonia.Common";
const windows = "RoyalApps.Community.Avalonia.Windows.NativeControls";
const api = collectPublicApiSurface([
  path.join(root, "src", common),
  path.join(root, "src", "RoyalApps.Community.Avalonia.Windows")
]);

test("both libraries contribute public types, excluding rendering and lifetime internals", () => {
  assert.deepEqual([...api.publicTypes].sort(), [
    `${common}.Behaviors.GridSplitterBehavior`,
    `${common}.Controls.AmbientGlowDecorator`,
    `${common}.Controls.CompositionRingSpinner`,
    `${windows}.IDisposeWinFormsControl`,
    `${windows}.WinFormsControlHost\`1`,
    `${windows}.WinFormsDisposeEventArgs`
  ].sort());
});

test("spinner exposes its styling contract without renderer internals", () => {
  const members = api.memberDetails.get(`${common}.Controls.CompositionRingSpinner`);
  for (const name of ["IsActive", "ForegroundColor", "TrackColor", "StrokeThickness"]) {
    assert.equal(members.get(name)?.kind, "property");
    assert.equal(members.get(`${name}Property`)?.kind, "field");
  }
  assert.equal(members.has("VisualState"), false);
  assert.equal(members.has("VisualHandler"), false);
});

test("inline accessors are documented as properties, not calls within their bodies", () => {
  const members = api.memberDetails.get(`${common}.Controls.AmbientGlowDecorator`);
  for (const name of ["AmbientColor", "AmbientColorLight", "AmbientColorDark", "EffectiveAmbientColor",
    "IsAnimationEnabled", "IsMotionAllowed", "CycleDuration", "HighlightThickness", "GlowOpacity"]) {
    assert.equal(members.get(name)?.kind, "property", name);
  }
  assert.equal(members.has("GetValue/1"), false);
  assert.equal(members.has("SetValue/2"), false);
  assert.equal(members.get("#ctor/0")?.kind, "method");
});

test("Avalonia property fields retain generic types without readonly/static modifiers", () => {
  const members = api.memberDetails.get(`${common}.Controls.AmbientGlowDecorator`);
  assert.deepEqual(members.get("EffectiveAmbientColorProperty"), {
    key: "EffectiveAmbientColorProperty", kind: "field", name: "EffectiveAmbientColorProperty",
    type: "DirectProperty<AmbientGlowDecorator, Color>"
  });
  const splitter = api.memberDetails.get(`${common}.Behaviors.GridSplitterBehavior`);
  assert.equal(splitter.get("EqualizeOnDoubleTappedProperty")?.type, "AttachedProperty<bool>");
  assert.equal(splitter.get("GetEqualizeOnDoubleTapped/1")?.returnType, "bool");
  assert.equal(splitter.get("SetEqualizeOnDoubleTapped/2")?.parameters.length, 2);
});

test("generic hosts use XML arity and expose their factory, not framework overrides", () => {
  const host = `${windows}.WinFormsControlHost\`1`;
  assert.equal(getDisplayTypeName(host), "WinFormsControlHost<T>");
  assert.deepEqual(api.typeInfos.get(host).typeParameters, ["T"]);
  assert.equal(api.memberDetails.get(host).get("Control")?.type, "T?");
  assert.equal(api.memberDetails.get(host).get("OnCreateWinFormsControl/0")?.returnType, "T?");
  assert.equal(api.publicMembers.get(host).has("CreateNativeControlCore"), false);
  assert.equal(api.publicMembers.get(host).has("DestroyNativeControlCore"), false);
});

test("interface events and multiline constructors preserve parameter metadata and type links", () => {
  const contract = `${windows}.IDisposeWinFormsControl`;
  assert.equal(api.memberDetails.get(contract).get("DisposeWinFormsControl")?.type,
    "EventHandler<WinFormsDisposeEventArgs>");
  const argumentsType = api.memberDetails.get(`${windows}.WinFormsDisposeEventArgs`);
  assert.equal(argumentsType.get("#ctor/1")?.parameters[0].name, "viewModel");
  assert.equal(argumentsType.get("#ctor/1")?.parameters[0].type, "IDisposeWinFormsControl");
  const rendered = renderTypeExpression("EventHandler<WinFormsDisposeEventArgs>", api);
  assert.match(rendered, /&lt;\[WinFormsDisposeEventArgs\]\(\/api\/reference\//);
  assert.ok(rendered.endsWith("&gt;"));
});
